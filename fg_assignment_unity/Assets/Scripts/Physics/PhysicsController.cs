using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Lander {
    namespace Physics {
        public class PhysicsController : MonoBehaviour, IPhysics {

            [Header("Acceleration")]
            [SerializeField] private AnimationCurve jolt;
            [SerializeField] private AnimationCurve lift;
            [SerializeField] private Vector3 controlAcceleration;
            [SerializeField] private Vector3 gravity;
            
            [Header("Velocity")]
            [SerializeField] private Vector3 maximumVelocity;
            
            [Header("Drag Coefficients")]
            [SerializeField][Min(0.1f)] private float minDragCoefficient = 50;
            [SerializeField][Min(0.1f)] private float maxDragCoefficient = 100;


            [Header("Collision raycasts")]
            [SerializeField][Min(2)] private int numOfCasts = 3;
            [SerializeField] private float raycastSkinWidth = 1;            

            private BoxCollider2D boxCollider;

            private Vector3 currentVelocity;
            private Vector3 externalAcceleration;
            private Vector3 inputDirection;
            private Vector3 targetFlightDirection;
            private float controlRate;
            private float dragCoefficientRate = 1;
            private bool isGrounded;

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

            public void Initialize() {
                boxCollider = GetComponent<BoxCollider2D>();
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
                externalAcceleration = Vector3.zero; // we reset all added external acceleration once consumed 
                
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
                
                var size = boxCollider.size;
                var heightDiff = size.y / numOfCasts;    
                var widthDiff = size.x / numOfCasts;                            
                var hitDifference = new List<Tuple<Vector3, Vector3>>();

                // raycast in direction of current x velocity direction
                var xSign = (vx == 0) ? 0 : Mathf.Sign(vx); 
                hitDifference = Cast(new Vector3(0, heightDiff, 0), new Vector3(xSign * size.x, -size.y, 0) , xSign * Vector3.right);
                if (hitDifference.Count > 0) {            
                    hitDifference = hitDifference.OrderBy( x => (x.Item1 - x.Item2).magnitude ).ToList();
                    if (hitDifference.Count > 0 && (hitDifference[0].Item1 - hitDifference[0].Item2).magnitude <= 0.01f) {
                        var mainPosition = hitDifference[0].Item2 - xSign * new Vector3(size.x / 2, 0, 0);
                        RaycastHit2D hitPos;
                        bool isHit = GetCollisionPoint(mainPosition, size.x, xSign * Vector3.right, out hitPos);
                        
                        if (isHit && !isGrounded) {
                            vx = -xSign * ((size.x / 2) - xSign * (hitPos.point.x - mainPosition.x) + 0.001f);                    
                        }                                                                                           
                    }          
                }

                // raycast to check if anything in the y direction is hit and find the cast that is has the shortest distance 
                var ySign = (vy == 0) ? 0 : Mathf.Sign(vy);
                hitDifference.Clear();
                hitDifference = Cast(new Vector3(widthDiff, 0, 0), new Vector3(-size.x, ySign == 0? -1 : ySign * size.y, 0), ySign * Vector3.up);

                if (hitDifference.Count > 0) { 
                    hitDifference = hitDifference.OrderBy( x => (x.Item1 - x.Item2).magnitude ).ToList();                       
                        
                    if ((hitDifference[0].Item1 - hitDifference[0].Item2).magnitude <= 0.01f) {
                        var mainPosition = hitDifference[0].Item2 - ySign * new Vector3(0, size.y / 2, 0);
                        RaycastHit2D hitPos;
                        var isHit = GetCollisionPoint(mainPosition, size.y, ySign * Vector3.up, out hitPos);
                        
                        if (isHit) {
                            if (ySign < 0) {
                                vx = 0;
                                vy = (size.y / 2) - ySign * (hitPos.point.y - mainPosition.y);
                            }                                                                                            
                            else if (ySign > 0) {
                                vy = 0;
                            }
                        }

                        if (ySign < 0) { 
                            isGrounded = true; 
                            return new Vector3(vx, vy, vz); 
                        }
                        else if (ySign > 0) { 
                            isGrounded = false; 
                        } 
                    }                                                 
                }
                else {            
                    isGrounded = false;
                }                    

                if (vy > 0) isGrounded = false;

                return new Vector3(vx, vy, vz);
            }

            private bool GetCollisionPoint(Vector3 mainPosition, float minCastSize, Vector3 castDirection, out RaycastHit2D point) {
                var hitInfos = Physics2D.RaycastAll(mainPosition, castDirection, minCastSize + raycastSkinWidth);
                hitInfos = hitInfos.Where(x => x.collider.gameObject != gameObject).OrderBy( x => ((Vector3) x.point - mainPosition).magnitude ).ToArray();
                // Debug.Log($"{castDirection}, {hitInfos.Length}");                
                if (hitInfos.Length > 0) point = hitInfos[0];
                else point = new RaycastHit2D();                
                return hitInfos.Length > 0;
            }

            private List<Tuple<Vector3, Vector3>> Cast(Vector3 interval, Vector3 size, Vector3 castDirection) {
                var hitDifference = new List<Tuple<Vector3, Vector3>>();
                for(int i = 0; i < numOfCasts; i++) {
                    var pos = transform.position + (Vector3)size / 2 + interval * i;  
                    var hitInfos = Physics2D.RaycastAll(pos, castDirection, raycastSkinWidth);
                    var orderedHits = hitInfos.Where(x => x.collider.gameObject != gameObject).OrderBy( x => ((Vector3) x.point - pos).magnitude ).ToArray();                        
                    foreach(var hit in orderedHits) {
                        hitDifference.Add(new Tuple<Vector3, Vector3>(hit.point, pos));                                                 
                    }
                }
                return hitDifference;
            }
        
        #if UNITY_EDITOR
            private void OnDrawGizmos() {
                if(boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
                var size = boxCollider.size;
                var widthDiff = size.x / (numOfCasts-1);
                var ySign = Mathf.Sign(currentVelocity.y);

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
        #endif
        }
    }
}

