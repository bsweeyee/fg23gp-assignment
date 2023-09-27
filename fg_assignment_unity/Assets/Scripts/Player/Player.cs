using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lander.Physics;
using Lander.GameState;
using Unity.VisualScripting;

namespace Lander {
    public class Player : MonoBehaviour, IEntities, IInput {
        [Header("Settings")]
        [SerializeField] private LayerMask physicsLayer;

        [Header("Flight")]
        [SerializeField][Range(0, 1)] private float flightDirectionControlValue;
        
        [Header("Boost")]
        [SerializeField][Range(0, 1)] private float boostDirectionControlValue;
        [SerializeField] private float boostSpeed;
        [SerializeField] private float boostFrames = 30;

        [Header("Death")]
        [SerializeField] private float speedDeathThreshold;
        [SerializeField] private int deathTriggerWaitFrames = 30;

        private PhysicsController physics;
        private float controlRate;
        private Vector3 movement;
        private bool isBoost;
        private int boostFrameCount = -1;
        private int boostFrameCooldown = -1;
        private int deathFrameCooldown = -1;

        public void Initialize(Game game) {
            physics = GetComponent<PhysicsController>();            
            gameObject.layer = (int)Mathf.Log(physicsLayer, 2);
            physics.Layer = physicsLayer;
        }

        public void OnFixedTick(Game game, float dt) {
            if (game.CurrentState == Game.PLAY_STATE) {
                EvaluateControlRate(movement, dt);
                
                physics.InputDirection = movement;
                physics.ControlRate = controlRate;
            }
        }

        public void OnTick(Game game, float dt) {
            if (game.CurrentState == Game.PLAY_STATE) {
                if (isBoost) {                
                    physics.AddAcceleration(EvaluateBoost(physics.CurrentVelocity));
                    controlRate = 0;                
                    movement = Vector3.zero;
                    isBoost = false;
                    boostFrameCount = 0;
                }
                
                if (boostFrameCount >= 0 && boostFrameCount <= boostFrames) {                
                    boostFrameCount++;
                } else {
                    boostFrameCount = -1;
                }  
                var obstacleHit = Physics2D.OverlapBox(transform.position, physics.SizeWithCast + Vector3.one * 0.1f, 0, ~physicsLayer);                

                // death check            
                if (obstacleHit != null) {                
                    if (deathFrameCooldown <= 0 && physics.CurrentVelocity.magnitude > speedDeathThreshold) {
                        // player animation, return to checkpoint                                            
                        game.CurrentState = Game.DEATH_STATE;
                        deathFrameCooldown = deathTriggerWaitFrames;
                        Debug.Log(deathFrameCooldown);
                        return;
                    }
                }

                // checkpoint save
                if (physics.IsGrounded) {
                    if (obstacleHit != null) {
                        var chk = obstacleHit.GetComponent<Checkpoint>();                    
                        if (chk != null) chk.SetCurrentSpawn();
                    }
                }

                deathFrameCooldown = Mathf.Clamp(deathFrameCooldown - 1, 0, deathTriggerWaitFrames);                                 
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
            if (movement.magnitude > 0) {
                controlRate += dt;
                controlRate = Mathf.Clamp01(controlRate);
            }
            else {
                controlRate = 0;
            }
        }
        
        private Vector3 EvaluateBoost(Vector3 currentVelocity) {
            var leftBoostDirection = Vector3.Lerp(Vector3.up, -Vector3.right, boostDirectionControlValue).normalized;
            var rightBoostDirection = Vector3.Lerp(Vector3.up, Vector3.right, boostDirectionControlValue).normalized; 
            var boostDirection = Vector3.zero;

            if (currentVelocity.y > 0) {            
                boostDirection = (Vector3.Dot(currentVelocity.normalized, Vector3.right) > 0) ? rightBoostDirection : leftBoostDirection;  
            } else {
                boostDirection = Random.Range(0, 2) == 0 ? Vector3.Lerp(Vector3.up, Vector3.right, boostDirectionControlValue).normalized : Vector3.Lerp(Vector3.up, -Vector3.right, boostDirectionControlValue).normalized;                             
            }            
            
            var boostAcceleration = boostDirection.normalized * boostSpeed;

            return boostAcceleration;
        }

        void IInput.Notify(InputData data) {
            if (movement.x != data.Movement.x) controlRate = 0;
            
            if (boostFrameCount < 0) {
                movement = data.Movement; 
                isBoost = data.Boost;
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

                Gizmos.color = Color.gray;
                Gizmos.DrawLine(transform.position, transform.position + ( Vector3.Lerp(Vector3.up, Vector3.right, boostDirectionControlValue).normalized * 0.5f ));
                Gizmos.DrawLine(transform.position, transform.position + ( Vector3.Lerp(Vector3.up, -Vector3.right, boostDirectionControlValue).normalized * 0.5f ));
            }

#endif
    }
}
