using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lander.Physics;
using Lander.GameState;
using UnityEngine.UI;

namespace Lander {
    public class Player : MonoBehaviour, IEntities, IInput, IDebug {
        [Header("Settings")]
        [SerializeField] private LayerMask physicsLayer;
        [SerializeField] private float inputWaitFrames = 30;

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

        [Header("Energy")]
        [SerializeField] private float maxEnergy;
        [SerializeField] private float energyBoostReductionRate;
        [SerializeField] private float energyFlightReductionRate;
        [SerializeField] private float energyRecoveryRate;

        [Header("Death")]
        [SerializeField] private float speedDeathThreshold;
        [SerializeField] private int deathTriggerWaitFrames = 30;

        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private PhysicsController physics;
        private float controlRate;
        private Vector3 movement;
        
        private float boostPowerRate;
        private InputData.EBoostState boostState;
        private int targetBoostDirection;

        private float currentEnergyLevel;                

        private int inputWaitFrameCount = -1;        
        private int deathFrameCooldown = -1;

        // TODO: move to a UI scripts
        private Transform arrowUI;
        private Image energyUI;

        public void Initialize(Game game) {
            physics = GetComponent<PhysicsController>();
            animator = GetComponentInChildren<Animator>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();            
            gameObject.layer = (int)Mathf.Log(physicsLayer, 2);
            physics.Layer = physicsLayer;
            targetBoostDirection = 1;

            currentEnergyLevel = maxEnergy;

            arrowUI = transform.Find("Arrow");
            arrowUI.gameObject.SetActive(false);

            energyUI = transform.Find("Energy/Bar").GetComponent<Image>();
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

                if (physics.IsGrounded) spriteRenderer.sprite = normal;
                else spriteRenderer.sprite = air;
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

                        arrowUI.gameObject.SetActive(true);
                        arrowUI.right = boostDirection.normalized;

                        animator.SetBool("isSqueeze", true);                        
                        break;
                    case InputData.EBoostState.RELEASED:
                        physics.AddAcceleration(EvaluateBoost());
                        controlRate = 0;                
                        movement = Vector3.zero;
                        inputWaitFrameCount = 0;
                        boostState = InputData.EBoostState.NONE;
                        boostPowerRate = 0;
                        arrowUI.gameObject.SetActive(false);
                        animator.SetBool("isSqueeze", false);
                        break;
                }                
                
                if (inputWaitFrameCount >= 0 && inputWaitFrameCount <= inputWaitFrames) {                
                    inputWaitFrameCount++;
                } else {
                    inputWaitFrameCount = -1;
                }

                // obstacle hit check  
                var obstacleHit = Physics2D.OverlapBox(transform.position, physics.SizeWithCast + Vector3.one * 0.1f, 0, ~physicsLayer);                

                // death check            
                if (obstacleHit != null) {                
                    if (deathFrameCooldown <= 0 && physics.CurrentVelocity.magnitude > speedDeathThreshold) {
                        // player animation, return to checkpoint                                            
                        game.CurrentState = Game.DEATH_STATE;
                        deathFrameCooldown = deathTriggerWaitFrames;
                        return;
                    }
                }

                // checkpoint save
                if (physics.IsGrounded) {
                    if (obstacleHit != null) {
                        var chk = obstacleHit.GetComponent<Checkpoint>();                    
                        if (chk != null) {
                            Checkpoint.CurrentSpawnWorldPosition = chk.SpawnWorldPosition;                             
                        }

                        if (boostState != InputData.EBoostState.PRESSED) currentEnergyLevel = Mathf.Clamp(currentEnergyLevel + (dt * energyRecoveryRate), 0, maxEnergy);                        
                    }
                }

                deathFrameCooldown = Mathf.Clamp(deathFrameCooldown - 1, 0, deathTriggerWaitFrames);

                energyUI.fillAmount = Mathf.InverseLerp(0, maxEnergy, currentEnergyLevel);                                 
            }
        }

        public void OnEnter(Game game, IBaseGameState previous, IBaseGameState current) {
            if (current.GetType() == typeof(DeathState)) {
                physics.Reset();
                Checkpoint.Respawn(transform);
                game.CurrentState = Game.PLAY_STATE;                
            }
        }

        public void OnExit(Game game, IBaseGameState previous, IBaseGameState current) {
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

        void IInput.Notify(InputData data) {
            if (movement.x != data.Movement.x) controlRate = 0;
            
            if (inputWaitFrameCount < 0) {
                movement = data.Movement; 
                boostState = data.BoostState;
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
