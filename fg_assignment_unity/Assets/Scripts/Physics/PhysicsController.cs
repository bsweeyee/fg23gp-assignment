using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Lander {
    namespace Physics {
        public class PhysicsController : MonoBehaviour, IPhysics, IDebug {

            [Header("Acceleration")]
            [SerializeField] private AnimationCurve jolt;
            [SerializeField] private AnimationCurve lift;
            [SerializeField] private Vector3 controlAcceleration;
            [SerializeField] private Vector3 gravity;
            [SerializeField][Range(0, 1)] private float externalAccelerationFalloff = 0.5f;
            
            [Header("Velocity")]
            [SerializeField] private Vector3 maximumVelocity;
            
            [Header("Drag Coefficients")]
            [SerializeField][Min(0.1f)] private float minDragCoefficient = 50;
            [SerializeField][Min(0.1f)] private float maxDragCoefficient = 100;


            [Header("Collision raycasts")]
            [SerializeField] private Vector3 size = Vector3.one;
            [SerializeField][Min(2)] private int numOfCasts = 3;
            [SerializeField] private float raycastSkinWidth = 1;                        

            private BoxCollider2D boxCollider2D;
            private Vector3 currentVelocity;
            private Vector3 externalAcceleration;
            private Vector3 inputDirection;
            private Vector3 targetFlightDirection;
            private float controlRate;
            private float dragCoefficientRate = 1;
            private bool isGrounded;
            private LayerMask layer;

            public Vector3 CurrentVelocity {
                get {
                    return currentVelocity;
                }
            }           

            public Vector3 InputDirection {                
                set {
                    inputDirection = value;
                }
            }

            public Vector3 TargetFlightDirection {
                set {
                    targetFlightDirection = value;
                }
            }

            public float ControlRate {
                set {
                    controlRate = value;
                }
            }

            public bool IsGrounded {
                get {
                    return isGrounded;
                }
            }

            public LayerMask Layer {
                set {
                    layer = value;
                }
            }

            public Vector3 Size {
                get { return size; }
            }

            public Vector3 SizeWithCast {
                get { return size + Vector3.one * raycastSkinWidth; }
            }            

            public void Initialize() {
                boxCollider2D = GetComponent<BoxCollider2D>();
                boxCollider2D.size = size;
            }

            public void OnFixedTick(float dt) {
                currentVelocity = EvaluateAcceleration(dt);        
                currentVelocity = EvaluateCollision();

                transform.position += currentVelocity;
                if (isGrounded) currentVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
            }

            public void OnTick(float dt) {

            }

            public void AddAcceleration(Vector3 a) {
                externalAcceleration += a;
                dragCoefficientRate = 0;
                currentVelocity = Vector3.zero;                
            }
            
            public void  Reset() {
                currentVelocity = Vector3.zero;
                externalAcceleration = Vector3.zero;
                controlRate = 0;
                dragCoefficientRate = 1;
                inputDirection = Vector3.zero;                            
            }

            private Vector3 EvaluateInput(Vector3 input, float totalDt) {
                var output = Vector3.zero;
                if (input.x < 0) {
                    output = Vector3.Lerp(-Vector3.right, targetFlightDirection, lift.Evaluate(totalDt));
                } else if (input.x > 0) {
                    output = Vector3.Lerp(Vector3.right, targetFlightDirection, lift.Evaluate(totalDt));
                }

                return output;
            }

            private Vector3 EvaluateDrag(Vector3 velocity, float dt) {
                var vMag = velocity.magnitude;
                var dragCo = Mathf.Lerp(minDragCoefficient, maxDragCoefficient, dragCoefficientRate / 2.0f);

                var drag = 0.5f * vMag * vMag * dragCo * velocity.normalized;
                drag = (drag.y < 0.01f) ? new Vector3(drag.x, 0, drag.z) : drag;
                drag = (drag.magnitude < 0.001f) ? Vector3.zero : drag;

                dragCoefficientRate = Mathf.Clamp(dragCoefficientRate + dt, 0, 2.0f);

                return drag;
            }

            private Vector3 EvaluateAcceleration(float dt) {
                var vx = currentVelocity.x;
                var vy = currentVelocity.y;
                var vz = currentVelocity.z;

                // modify input controls
                var input = EvaluateInput(inputDirection, controlRate);
                
                // calculate drag using formula 1/2*v^2*C
                var drag = EvaluateDrag(currentVelocity, dt);
                
                // calculate acceleration
                var cA = Vector3.Lerp( controlAcceleration * 0.1f , controlAcceleration, jolt.Evaluate(controlRate));
                
                var finalAcceleration = new Vector3(cA.x * input.x, cA.y * input.y, cA.z * input.z); 
                finalAcceleration -= drag;                
                finalAcceleration += externalAcceleration;
                externalAcceleration *= (1 - externalAccelerationFalloff); // we reduce external acceleration by factor 
                
                if(!isGrounded && Mathf.Abs(vy) < maximumVelocity.y) {
                    finalAcceleration += gravity;
                }                

                return new Vector3(vx, vy, vz) + (finalAcceleration * dt);
            }

            private Vector3 EvaluateCollision() {
                var vx = currentVelocity.x;
                var vy = currentVelocity.y;
                var vz = currentVelocity.z;

                if ( Mathf.Abs(vx) <= 0.01f) { vx = 0; } 
                                
                var heightDiff = size.y / (numOfCasts-1);    
                var widthDiff = size.x / (numOfCasts-1);                            

                // raycast in direction of current x velocity direction
                var xSign = (vx == 0) ? 0 : Mathf.Sign(vx); 
                RaycastHit2D hitX;
                var isHit2DX = Cast2D(new Vector3(0, heightDiff, 0), new Vector3(xSign * size.x, -size.y, 0) , xSign * Vector3.right, out hitX);
                if (isHit2DX) {
                    vx = (hitX.distance - raycastSkinWidth) * xSign;
                }                

                // raycast to check if anything in the y direction is hit and find the cast that is has the shortest distance 
                var ySign = (Mathf.Abs(vy) <= 0.0005f && isGrounded) ? 0 : Mathf.Sign(vy);                
                RaycastHit2D hitY;
                if (ySign == 0) {                    
                    isGrounded = true;
                }
                else {
                    var isHit2DY = Cast2D(new Vector3(widthDiff, 0, 0), new Vector3(-size.x, ySign * size.y, 0), ySign * Vector3.up, out hitY);
                    if (isHit2DY) {                    
                        vy = (hitY.distance - raycastSkinWidth) * ySign;
                        if (ySign < 0) { 
                            vx = 0;                                                                                    
                            isGrounded = true;                         
                        }
                        else {
                            vy = 0;                            
                            isGrounded = false;
                        }                                         
                    }
                    else {                        
                        isGrounded = false;
                    }
                }

                // check overlap                
                var hitCollider = Physics2D.OverlapBox(transform.position, size, 0, ~layer);
                if (hitCollider) {
                    var separation = hitCollider.Distance(boxCollider2D);
                    // Debug.Log($"separation: {separation.normal}, {separation.distance}");
                    var s = -(Vector3)separation.normal * separation.distance;
                    vx += s.x;
                    vy += s.y;
                    vz += s.z;
                }                

                return new Vector3(vx, vy, vz);
            }

            private bool Cast2D(Vector3 interval, Vector3 size, Vector3 castDirection, out RaycastHit2D hit) {
                hit = new RaycastHit2D();
                hit.distance = float.MaxValue;
                bool isHit = false;
                for(int i = 0; i < numOfCasts; i++) {
                    var pos = transform.position + ((Vector3)size / 2) + interval * i;  
                    var testHit = Physics2D.Raycast(pos, castDirection, raycastSkinWidth, ~layer);                    
                    if (testHit.collider) {
                        if (testHit.collider != null && testHit.distance <= hit.distance) {
                            // Debug.Log($"cast: {i}, {hit.distance}, {testHit.distance}");
                            hit = testHit;
                        }
                        isHit = true;                            
                    }
                }
                return isHit;                                
            }
        
        #if UNITY_EDITOR
            private void OnDrawGizmos() {                
                var widthDiff = size.x / (numOfCasts-1);
                var ySign = Mathf.Sign(currentVelocity.y);

                // draw collision box
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position, size);

                // y cast
                for(int i = 0; i < numOfCasts; i++) {
                    var pos = transform.position + new Vector3(-size.x, ySign * size.y, 0) / 2 + new Vector3(widthDiff * i, 0, 0);
                    Gizmos.color = Color.magenta;
                    if (currentVelocity.y != 0) {
                        Gizmos.DrawLine(pos, pos + (ySign * Vector3.up * raycastSkinWidth));
                    }
                }

                // draw current velocity
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + (currentVelocity.normalized * 0.5f));                
            }

            public void OnDrawGUI() {
                string vel = $"current velocity: {currentVelocity}, {currentVelocity.magnitude}";
                
                GUILayout.Label(vel);
            }
#endif
        }
    }
}

