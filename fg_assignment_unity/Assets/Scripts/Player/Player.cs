using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lander.Physics;
using Lander.GameState;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Lander {
    public class Player : MonoBehaviour, ILevelStartEntity, ILevelPlayEntity, IInput, IDebug {
        public enum EPlayerState {
            NONE,
            ALIVE,
            DEAD,
        }

        [Header("Settings")]
        [SerializeField] private LayerMask physicsLayer;

        [Header("Sprite")]
        [SerializeField] private Sprite normal;
        [SerializeField] private Sprite air;

        [Header("Controls")]
        [SerializeField] private AnimationCurve jolt;
        [SerializeField] private AnimationCurve lift;
        [SerializeField] private Vector3 controlAcceleration;
        [SerializeField] private Vector3 gravity;

        [Header("Flight")]
        [SerializeField][Range(0, 1)] private float flightDirectionControlValue;

        [Header("Boost")]
        [SerializeField][Range(0, 1)] private float boostMinimumDirection;
        [SerializeField][Range(0, 1)] private float boostMaximumDirection;
        [SerializeField] private float boostAngleSpeed;
        [SerializeField] private float boostMoveSpeed;
        [SerializeField] private int numOfBoosts;
        [SerializeField] private bool isAllowAirBoost;

        [Header("Energy")]
        [SerializeField] private float maxEnergy;
        [SerializeField] private float energyBoostReductionRate;
        [SerializeField] private float energyFlightReductionRate;
        [SerializeField] private float energyRecoveryRate;

        [Header("Death")]
        [SerializeField] private float speedDeathThreshold;
        [SerializeField] private float deathAnimationTime = 1;
        [SerializeField] private int deathTriggerWaitFrames = 30;

        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private PhysicsController physics;
        private float controlRate;
        private Vector3 movement;

        private bool wasReleased;
        private float boostPowerRate;
        private InputData.EBoostState boostState;
        private int targetBoostDirection;
        private Vector3 targetFlightDirection;

        private float currentEnergyLevel;
        private int currentNumOfBoosts;

        private int deathFrameCooldown = -1;
        private float deathTimePeriod = 0;

        private Game game;
        private IPlayerObserver[] observers;

        private EPlayerState currentPlayerState;

        public int CurrentNumOfBoosts {
            get {
                return currentNumOfBoosts;
            }
            set {
                currentNumOfBoosts = Mathf.Clamp(value, 0, int.MaxValue);
                foreach(var p in observers) {
                    p.OnBoostAmountChange(currentNumOfBoosts, 0, numOfBoosts);
                }
            }
        }

        public EPlayerState CurrentPlayerState {
            get { return currentPlayerState; }
            set {
                var previous = currentPlayerState;
                currentPlayerState = value;

                switch (previous) {
                    case EPlayerState.ALIVE:
                    break;
                    case EPlayerState.DEAD:
                    ExitDead(game);
                    break;
                }

                switch (currentPlayerState) {
                    case EPlayerState.ALIVE:
                    break;
                    case EPlayerState.DEAD:
                    EnterDead(game);
                    break;
                }

                foreach(var s in observers) {
                    s.OnExitState(previous, currentPlayerState);
                }
                foreach(var s in observers) {
                    s.OnEnterState(previous, currentPlayerState);
                }
            }
        }

        public bool IsEarlyInitialized { get; private set; }

        public bool IsLateInitialized { get; private set; }

        bool IGameInitializeEntity.IsEarlyInitialized => throw new System.NotImplementedException();

        bool IGameInitializeEntity.IsLateInitialized => throw new System.NotImplementedException();

        public void EarlyInitialize(Game game) {
            if (IsEarlyInitialized) return;

            physics = GetComponent<PhysicsController>();
            animator = GetComponentInChildren<Animator>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            gameObject.layer = (int)Mathf.Log(game.GameSettings.PlayerLayer, 2);

            targetBoostDirection = 1;

            currentEnergyLevel = maxEnergy;
            currentNumOfBoosts = numOfBoosts;

            observers = GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<IPlayerObserver>().ToArray();

            this.game = game;
            IsEarlyInitialized = true;
        }

        public void LateInitialize(Game game) {
            if (IsLateInitialized) return;

            physics.Layer = game.GameSettings.ObstacleLayer;
            physics.OnFirstGrounded.AddListener( OnFirstGrounded );
            physics.OnFirstUnGrounded.AddListener( OnFirstUnGrounded );

            CurrentPlayerState = EPlayerState.ALIVE;

            IsLateInitialized = true;
        }

        void ILevelPlayEntity.OnFixedTick(Game game, float dt) {
            switch(CurrentPlayerState) {
                case EPlayerState.ALIVE:
                FixedTickAlive(game, dt);
                break;
            }
        }

        void ILevelPlayEntity.OnTick(Game game, float dt) {
            switch(CurrentPlayerState)
            {
                case EPlayerState.ALIVE:
                TickAlive(game, dt);
                break;
                case EPlayerState.DEAD:
                TickDead(game, dt);
                break;
            }
        }

        void ILevelPlayEntity.OnEnter(Game game, IBaseGameState previous) {
            physics.Gravity = gravity;
            physics.Reset();
            controlRate = 0;
            targetFlightDirection = Vector3.zero;
        }

        void ILevelPlayEntity.OnExit(Game game, IBaseGameState current) {
        }

        void ILevelStartEntity.OnEnter(Game game, IBaseGameState previous) {
            currentEnergyLevel = maxEnergy;
            game.PhysicsTickFactor = 1;         
        }

        void ILevelStartEntity.OnExit(Game game, IBaseGameState current) {
        }

        void ILevelStartEntity.OnTick(Game game, float dt) {
        }
 
        void ILevelStartEntity.OnFixedTick(Game game, float dt) {
        }

        private void FixedTickAlive(Game game, float dt) {
            if (currentEnergyLevel <= 0) {
                physics.Input = Vector3.zero;
            }
            else {
                controlRate = EvaluateControlRate(movement, dt);
                physics.Input = EvaluateInput(movement, controlRate);

                if (movement.magnitude > 0) {
                    currentEnergyLevel = Mathf.Clamp(currentEnergyLevel - (dt * energyFlightReductionRate), 0, maxEnergy);
                }

                if (movement.x > 0) spriteRenderer.gameObject.transform.right = Vector3.right;
                else if (movement.x < 0) spriteRenderer.gameObject.transform.right = -Vector3.right;
            }

            foreach(var p in observers) {
                p.OnVelocityChange(physics.CurrentVelocity, speedDeathThreshold);
            }
        }

        private void TickAlive(Game game, float dt) {
            switch (boostState) {
                case InputData.EBoostState.PRESSED:
                    if (currentEnergyLevel <= 0) break;

                    boostPowerRate = Mathf.Clamp01(boostPowerRate + (dt * boostAngleSpeed));
                    currentEnergyLevel = (boostPowerRate < 1) ? Mathf.Clamp(currentEnergyLevel - (dt * energyBoostReductionRate), 0, maxEnergy) : currentEnergyLevel;

                    var minBoostDir = Vector3.Lerp(Vector3.up, targetBoostDirection * Vector3.right, boostMinimumDirection);
                    var maxBoostDir = Vector3.Lerp(Vector3.up, targetBoostDirection * Vector3.right, boostMaximumDirection);

                    var boostDirection = Vector3.Lerp(minBoostDir, maxBoostDir, boostPowerRate).normalized;

                    animator.SetBool("isSqueeze", true);
                    foreach(var p in observers) {
                        p.OnBoostDirectionChange(boostDirection);
                    }
                    break;
                case InputData.EBoostState.RELEASED:
                    physics.AddAcceleration(EvaluateBoost());
                    controlRate = 0;
                    boostPowerRate = 0;
                    wasReleased = true;
                    movement = Vector3.zero;
                    boostState = InputData.EBoostState.NONE;
                    if(!physics.IsGrounded) {
                        CurrentNumOfBoosts -= 1;
                    }

                    foreach(var p in observers) {
                        p.OnBoostDirectionChange(Vector3.zero);
                    }
                    animator.SetBool("isSqueeze", false);
                    break;
            }

            var obstacleHit = Physics2D.OverlapBox(transform.position, physics.SizeWithCast + Vector3.one * 0.1f, 0, game.GameSettings.ObstacleLayer);

            // death check
            if (obstacleHit != null) {
                if (deathFrameCooldown <= 0 && physics.CurrentVelocity.magnitude > speedDeathThreshold) {
                    // player animation, return to checkpoint
                    CurrentPlayerState = EPlayerState.DEAD;
                    return;
                }
            }

            deathFrameCooldown = Mathf.Clamp(deathFrameCooldown - 1, 0, deathTriggerWaitFrames);

            // grounded check
            if (physics.IsGrounded) {
                if (obstacleHit != null) {
                    if (boostState != InputData.EBoostState.PRESSED) {
                        currentEnergyLevel = Mathf.Clamp(currentEnergyLevel + (dt * energyRecoveryRate), 0, maxEnergy);
                    }
                    var chk = obstacleHit.GetComponent<Checkpoint>();
                    if (chk != null) {
                        Checkpoint.CurrentSpawnWorldPosition = chk.SpawnWorldPosition;
                    }
                }
            }

            foreach(var p in observers) {
                p.OnEnergyChange(currentEnergyLevel, 0, maxEnergy);
            }
        }

        private void TickDead(Game game, float dt) {
            spriteRenderer.transform.rotation = Quaternion.AngleAxis(Mathf.Lerp(0, 270, deathTimePeriod / deathAnimationTime), transform.forward);

            deathTimePeriod += dt;

            if (deathTimePeriod > deathAnimationTime) {
                Checkpoint.Respawn(transform);
                CurrentPlayerState = EPlayerState.ALIVE;
            }
        }

        private void ExitDead(Game game) {
            spriteRenderer.transform.rotation = Quaternion.identity;
            targetBoostDirection = 1;
            deathFrameCooldown = deathTriggerWaitFrames;
            CurrentNumOfBoosts = numOfBoosts;
            currentEnergyLevel = maxEnergy;
            deathTimePeriod = 0;
            animator.SetBool("isDead", false);
        }

        private void EnterDead(Game game) {
            animator.SetBool("isDead", true);
            physics.Reset();
            controlRate = 0;
            targetFlightDirection = Vector3.zero;
        }

        private float EvaluateControlRate(Vector3 movement, float dt) {
            float output = 0;
            // add to control dt
            if (movement.x > 0) targetBoostDirection = 1;
            else if (movement.x < 0) targetBoostDirection = -1;

            if (movement.magnitude > 0) {
                output = controlRate + dt;
                output = Mathf.Clamp01(output);
            }
            else {
                output = 0;
            }
            return output;
        }

        private Vector3 EvaluateBoost() {
            var minBoostDir = Vector3.Lerp(Vector3.up, targetBoostDirection * Vector3.right, boostMinimumDirection);
            var maxBoostDir = Vector3.Lerp(Vector3.up, targetBoostDirection * Vector3.right, boostMaximumDirection);

            var boostDirection = Vector3.Lerp(minBoostDir, maxBoostDir, boostPowerRate).normalized;
            var boostSpeed = Mathf.Lerp(0, boostMoveSpeed, boostPowerRate);

            var boostAcceleration = boostDirection * boostSpeed;

            return boostAcceleration;
        }

        private Vector3 EvaluateInput(Vector3 input, float totalDt) {
            var final = Vector3.zero;
            if (input.x < 0) {
                final = Vector3.Lerp(-Vector3.right, targetFlightDirection, lift.Evaluate(totalDt));
            } else if (input.x > 0) {
                final = Vector3.Lerp(Vector3.right, targetFlightDirection, lift.Evaluate(totalDt));
            }

            var cA = Vector3.Lerp( controlAcceleration * 0.1f , controlAcceleration, jolt.Evaluate(controlRate));

            return new Vector3(cA.x * final.x, cA.y * final.y, cA.z * final.z);;
        }

        private void OnFirstGrounded() {
            spriteRenderer.sprite = normal;
            CurrentNumOfBoosts = numOfBoosts;
        }

        private void OnFirstUnGrounded() {
            spriteRenderer.sprite = air;
            if (wasReleased) {
                CurrentNumOfBoosts -= 1;
                wasReleased = false;
            }
        }

        void IInput.Notify(InputData data) {
            if (data.Paused || game.CurrentState != Game.PLAY_STATE) return;

            if (movement.x != data.Movement.x) controlRate = 0;

            if (boostState != InputData.EBoostState.PRESSED) movement = data.Movement;

            boostState = data.BoostState;
            if (boostState == InputData.EBoostState.PRESSED || boostState == InputData.EBoostState.RELEASED) {
                if (CurrentNumOfBoosts <= 0) { boostState = InputData.EBoostState.NONE; }
                if (!isAllowAirBoost && !physics.IsGrounded) { boostState = InputData.EBoostState.NONE; }
            }

            if (movement.x < 0) {
                targetFlightDirection = Vector3.Lerp(Vector3.up, -Vector3.right, flightDirectionControlValue).normalized;
            } else if (movement.x > 0) {
                targetFlightDirection = Vector3.Lerp(Vector3.up, Vector3.right, flightDirectionControlValue).normalized;
            }
        }


#if UNITY_EDITOR
        private void OnDrawGizmos() {
            // draw target velocity
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + ( Vector3.Lerp(Vector3.up, Vector3.right, flightDirectionControlValue).normalized * 0.5f ));
            Gizmos.DrawLine(transform.position, transform.position + ( Vector3.Lerp(Vector3.up, -Vector3.right, flightDirectionControlValue).normalized * 0.5f ));

            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position + ( Vector3.Lerp(Vector3.up, Vector3.right, boostMaximumDirection).normalized * 0.5f ));
            Gizmos.DrawLine(transform.position, transform.position + ( Vector3.Lerp(Vector3.up, -Vector3.right, boostMaximumDirection).normalized * 0.5f ));

            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.position, transform.position + ( Vector3.Lerp(Vector3.up, Vector3.right, boostMinimumDirection).normalized * 0.5f ));
            Gizmos.DrawLine(transform.position, transform.position + ( Vector3.Lerp(Vector3.up, -Vector3.right, boostMinimumDirection).normalized * 0.5f ));

            var minBoostDir = Vector3.Lerp(Vector3.up, targetBoostDirection * Vector3.right, boostMinimumDirection);
            var maxBoostDir = Vector3.Lerp(Vector3.up, targetBoostDirection * Vector3.right, boostMaximumDirection);

            var boostDirection = Vector3.Lerp(minBoostDir, maxBoostDir, boostPowerRate).normalized;

            Gizmos.color = Color.grey;
            Gizmos.DrawLine(transform.position, transform.position + ( boostDirection.normalized * 0.5f ));
        }

        public void OnDrawGUI() {
            string vel = $"current velocity: {physics.CurrentVelocity}, {physics.CurrentVelocity.magnitude}";
                
            GUILayout.Label(vel);   
            
            string energy = $"energy level: {currentEnergyLevel}";

            GUILayout.Label(energy);
        }
#endif
    }
}
