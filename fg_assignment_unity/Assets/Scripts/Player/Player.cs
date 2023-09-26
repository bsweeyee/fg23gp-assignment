using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lander.Physics;

namespace Lander {
    public class Player : MonoBehaviour, IEntities, IInput {
        [SerializeField][Range(0, 1)] private float flightDirectionControlValue;
        [SerializeField][Range(0, 1)] private float boostDirectionControlValue;
        [SerializeField] private float boostSpeed;
        [SerializeField] private float boostFrames = 30;

        private PhysicsController physics;
        private float controlRate;
        private Vector3 movement;
        private bool isBoost;
        private int boostFrameCount = -1;

        public void Initialize() {
            physics = GetComponent<PhysicsController>();
        }

        public void FixedTick(float dt) {
            EvaluateControlRate(movement, dt);
            
            physics.InputDirection = movement;
            physics.ControlRate = controlRate;
        }

        public void Tick(float dt) {
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
