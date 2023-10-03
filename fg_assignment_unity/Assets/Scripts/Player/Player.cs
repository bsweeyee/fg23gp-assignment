using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lander.Physics;
using Lander.GameState;
using System.Linq;
using UnityEngine.UI;

namespace Lander {
    public class Player : MonoBehaviour, IGameStateEntity, IInput, IDebug {
        [Header("Settings")]
        [SerializeField] private LayerMask physicsLayer;

        [Header("Sprite")]
        [SerializeField] private Sprite normal;
        [SerializeField] private Sprite air;

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

        private float currentEnergyLevel;
        private int currentNumOfBoosts;

        private int deathFrameCooldown = -1;
        private float deathTimePeriod = 0;

        private Game game;
        private IPlayerObserver[] observers;

        private int CurrentNumOfBoosts {
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

        public void EarlyInitialize(Game game) {
            physics = GetComponent<PhysicsController>();
            animator = GetComponentInChildren<Animator>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            gameObject.layer = (int)Mathf.Log(physicsLayer, 2);

            targetBoostDirection = 1;

            currentEnergyLevel = maxEnergy;
            currentNumOfBoosts = numOfBoosts;

            observers = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IPlayerObserver>().ToArray();

            this.game = game;
        }

        public void LateInitialize(Game game) {
            physics.Layer = physicsLayer;
            physics.OnFirstGrounded.AddListener( OnFirstGrounded );
            physics.OnFirstUnGrounded.AddListener( OnFirstUnGrounded );
        }

        public void OnFixedTick(Game game, float dt) {
            if (game.CurrentState == Game.PLAY_STATE) {
                if (currentEnergyLevel <= 0) {
                    physics.InputDirection = Vector3.zero;
                    physics.ControlRate = 0;
                    return;
                }

                EvaluateControlRate(movement, dt);

                physics.InputDirection = movement;
                physics.ControlRate = controlRate;

                if (movement.magnitude > 0) {
                    currentEnergyLevel = Mathf.Clamp(currentEnergyLevel - (dt * energyFlightReductionRate), 0, maxEnergy);
                }

                if (movement.x > 0) spriteRenderer.gameObject.transform.right = Vector3.right;
                else if (movement.x < 0) spriteRenderer.gameObject.transform.right = -Vector3.right;

                foreach(var p in observers) {
                    p.OnVelocityChange(physics.CurrentVelocity);
                }
            }
        }

        public void OnTick(Game game, float dt) {
            if (game.CurrentState == Game.PLAY_STATE) {
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

                var obstacleHit = Physics2D.OverlapBox(transform.position, physics.SizeWithCast + Vector3.one * 0.1f, 0, ~physicsLayer);

                // death check
                if (obstacleHit != null) {
                    if (deathFrameCooldown <= 0 && physics.CurrentVelocity.magnitude > speedDeathThreshold) {
                        // player animation, return to checkpoint
                        game.CurrentState = Game.DEATH_STATE;
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
            else if(game.CurrentState == Game.DEATH_STATE) {

                spriteRenderer.transform.rotation = Quaternion.AngleAxis(Mathf.Lerp(0, 270, deathTimePeriod / deathAnimationTime), transform.forward);

                deathTimePeriod += dt;

                if (deathTimePeriod > deathAnimationTime) {
                    Checkpoint.Respawn(transform);
                    game.CurrentState = Game.PLAY_STATE;
                }
            }
        }

        public void OnEnter(Game game, IBaseGameState previous, IBaseGameState current) {
            if (current.GetType() == typeof(DeathState)) {
                animator.SetBool("isDead", true);
                physics.Reset();
            }
        }

        public void OnExit(Game game, IBaseGameState previous, IBaseGameState current) {
            if (previous.GetType() == typeof(DeathState)) {
                spriteRenderer.transform.rotation = Quaternion.identity;
                targetBoostDirection = 1;
                deathFrameCooldown = deathTriggerWaitFrames;
                CurrentNumOfBoosts = numOfBoosts;
                currentEnergyLevel = maxEnergy;
                deathTimePeriod = 0;
                animator.SetBool("isDead", false);
            }
        }

        private void EvaluateControlRate(Vector3 movement, float dt) {
            // add to control dt
            if (movement.x > 0) targetBoostDirection = 1;
            else if (movement.x < 0) targetBoostDirection = -1;

            if (movement.magnitude > 0) {
                controlRate += dt;
                controlRate = Mathf.Clamp01(controlRate);
            }
            else {
                controlRate = 0;
            }
        }

        private Vector3 EvaluateBoost() {
            var minBoostDir = Vector3.Lerp(Vector3.up, targetBoostDirection * Vector3.right, boostMinimumDirection);
            var maxBoostDir = Vector3.Lerp(Vector3.up, targetBoostDirection * Vector3.right, boostMaximumDirection);

            var boostDirection = Vector3.Lerp(minBoostDir, maxBoostDir, boostPowerRate).normalized;
            var boostSpeed = Mathf.Lerp(0, boostMoveSpeed, boostPowerRate);

            var boostAcceleration = boostDirection * boostSpeed;

            return boostAcceleration;
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
            if (movement.x != data.Movement.x) controlRate = 0;

            if (boostState != InputData.EBoostState.PRESSED) movement = data.Movement;

            boostState = data.BoostState;
            if (boostState == InputData.EBoostState.PRESSED) {
                if (CurrentNumOfBoosts <= 0) { boostState = InputData.EBoostState.NONE; }
                if (!isAllowAirBoost && !physics.IsGrounded) { boostState = InputData.EBoostState.NONE; }
            }

            if (movement.x < 0) {
                physics.TargetFlightDirection = Vector3.Lerp(Vector3.up, -Vector3.right, flightDirectionControlValue).normalized;
            } else if (movement.x > 0) {
                physics.TargetFlightDirection = Vector3.Lerp(Vector3.up, Vector3.right, flightDirectionControlValue).normalized;
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
            string energy = $"energy level: {currentEnergyLevel}";

            GUILayout.Label(energy);
        }

#endif
    }
}
