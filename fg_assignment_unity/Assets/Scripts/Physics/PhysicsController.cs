using UnityEngine;
using UnityEngine.Events;

namespace Lander {
    namespace Physics {
        public class PhysicsController : MonoBehaviour, IPhysics {

            [Header("Acceleration")]
            [SerializeField] private Vector3 gravity;
            [SerializeField] private AnimationCurve externalAccelerationFalloffCurve;
            [SerializeField] private float fallOffSpeed = 1;

            [Header("Velocity")]
            [SerializeField] private Vector3 maximumVelocity;
            
            [Header("Drag Coefficients")]
            [SerializeField][Min(0.1f)] private float minDragCoefficient = 50;
            [SerializeField][Min(0.1f)] private float maxDragCoefficient = 100;


            [Header("Collision raycasts")]
            [SerializeField] private Vector3 size = Vector3.one;
            [SerializeField][Min(2)] private int numOfCasts = 3;
            [SerializeField] private float raycastSkinWidth = 1;                        
            [SerializeField] private LayerMask layer;

            private BoxCollider2D boxCollider2D;
            private Vector3 currentVelocity;
            private Vector3 externalAcceleration;
            private Vector3 input;
            private float dragCoefficientRate = 1;
            private float externalAccelerationFallOffRate;
            private bool isGrounded;

            private UnityEvent onFirstGrounded;
            private UnityEvent onFirstUnGrounded;
            
            private Vector3 currentRaycastSkinWidth;

            public Vector3 Gravity {
                get { return gravity; }
                set { gravity = value; }
            }

            public Vector3 CurrentVelocity {
                get {
                    return currentVelocity;
                }
                set {
                    currentVelocity = value;
                }                
            }           

            public Vector3 Input {                
                set {
                    input = value;
                }
            }            

            public bool IsGrounded {
                get {
                    return isGrounded;
                }
                protected set {
                    if (isGrounded == false && value == true) onFirstGrounded.Invoke();
                    if (isGrounded == true && value == false) onFirstUnGrounded.Invoke();

                    isGrounded = value;
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

            public UnityEvent OnFirstGrounded {
                get { return onFirstGrounded; }
            }

            public UnityEvent OnFirstUnGrounded {
                get { return onFirstUnGrounded; }
            }

            public bool IsEarlyInitialized { get; private set; }

            public bool IsLateInitialized { get; private set; }

            public void EarlyInitialize(Game game) {
                if (IsEarlyInitialized) return;

                boxCollider2D = GetComponent<BoxCollider2D>();
                boxCollider2D.size = size;

                onFirstGrounded = new UnityEvent();
                onFirstUnGrounded = new UnityEvent();

                currentRaycastSkinWidth = new Vector3(raycastSkinWidth, raycastSkinWidth, raycastSkinWidth);

                IsEarlyInitialized = true;
            }

            public void LateInitialize(Game game) {
                if (IsLateInitialized) return;

                IsLateInitialized = true;                
            }           

            public void OnFixedTick(Game game, float dt) {
                if (!gameObject.activeSelf) return;

                currentVelocity = EvaluateAcceleration(dt);        
                currentVelocity = EvaluateCollision(dt);
                
                transform.position += currentVelocity * dt;
            }

            public void OnTick(Game game, float dt) {

            }

            public void AddAcceleration(Vector3 a) {
                externalAcceleration += a;
                dragCoefficientRate = 0;
                currentVelocity = Vector3.zero;
            }
            
            public void Reset() {
                currentVelocity = Vector3.zero;
                externalAcceleration = Vector3.zero;                
                dragCoefficientRate = 1;
                externalAccelerationFallOffRate = 0;
                input = Vector3.zero;                                            
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
                // calculate drag using formula 1/2*v^2*C
                var drag = EvaluateDrag(currentVelocity, dt);
                
                // calculate acceleration
                var finalExternalAcceleration = Vector3.zero;             
                if (externalAcceleration.magnitude > 0) {
                    externalAccelerationFallOffRate += dt * fallOffSpeed;
                    var t = externalAccelerationFalloffCurve.Evaluate(externalAccelerationFallOffRate);
                    finalExternalAcceleration = Vector3.Lerp(Vector3.zero, externalAcceleration, t);                                                            
                }
                if (finalExternalAcceleration.magnitude < 0.01f) {
                    externalAcceleration = Vector3.zero;
                    externalAccelerationFallOffRate = 0;
                }

                var finalAcceleration = input;                 
                finalAcceleration -= drag;                
                finalAcceleration += finalExternalAcceleration;                
                finalAcceleration += gravity;                                            

                var finalVel = new Vector3(vx, vy, vz) + (finalAcceleration * dt);
                if (finalVel.magnitude > maximumVelocity.magnitude) finalVel = currentVelocity;
                return finalVel;
            }

            private Vector3 EvaluateCollision(float dt) {
                var vx = currentVelocity.x;
                var vy = currentVelocity.y;
                var vz = currentVelocity.z;

                var newVelocity = Vector3.zero;

                if ( Mathf.Abs(vx) <= 0.01f) { vx = 0; } 
                                
                var heightDiff = size.y / (numOfCasts-1);    
                var widthDiff = size.x / (numOfCasts-1);                                                           

                // raycast in direction of current x velocity direction
                var xSign = (Mathf.Abs(vx) <= float.Epsilon) ? 0 : Mathf.Sign(vx);
                // raycast to check if anything in the y direction is hit and find the cast that is has the shortest distance 
                var ySign = (Mathf.Abs(vy) <= Mathf.Epsilon) ? -1 : Mathf.Sign(vy);                                                       
                
                RaycastHit2D hitY;               
                var isHit2DY = Cast2D(transform.position + (newVelocity * dt),  new Vector3(widthDiff, 0, 0), new Vector3(-size.x, ySign * size.y, 0), ySign * Vector3.up, currentRaycastSkinWidth.y, out hitY);
                RaycastHit2D hitX;
                var isHit2DX = Cast2D(transform.position + (newVelocity * dt), new Vector3(0, heightDiff, 0), new Vector3(xSign * size.x, -size.y, 0) , xSign * Vector3.right, currentRaycastSkinWidth.x, out hitX);                
                
                // if (isHit2DY) Debug.Log("ground first: " + vy);
                
                if (isHit2DX) {
                    currentRaycastSkinWidth.x = hitX.distance;
                    transform.position += new Vector3(xSign * (currentRaycastSkinWidth.x + (xSign * hitX.normal.x * 0.1f)), 0, 0);                    
                    vx = 0;
                }
                else {
                    currentRaycastSkinWidth.x = raycastSkinWidth;
                }                

                
                if (isHit2DY) {                    
                    currentRaycastSkinWidth.y = hitY.distance;
                    if (hitY.distance - (ySign * vy) < 0) {
                        if (ySign < 0) {
                            vx = 0;
                            IsGrounded = true;                            
                        }
                        else if (ySign > 0) {
                            var testHit = Physics2D.Raycast(transform.position, xSign * Vector3.right, size.x + currentRaycastSkinWidth.x, layer);                    
                            if (testHit.collider == null) {
                                vx = currentVelocity.x;
                            }
                        }
                        vy = 0;
                        transform.position += new Vector3(0, ySign * currentRaycastSkinWidth.y, 0);
                    }                                                               
                }
                else {
                    currentRaycastSkinWidth.y = raycastSkinWidth;                                                
                    IsGrounded = false;                                               
                }                                                 

                newVelocity.x = vx;
                newVelocity.y = vy;                               
               
                return newVelocity;
            }

            private bool Cast2D(Vector3 position, Vector3 interval, Vector3 size, Vector3 castDirection, float castWidth, out RaycastHit2D hit) {
                hit = new RaycastHit2D();
                hit.distance = float.MaxValue;
                bool isHit = false;
                for(int i = 0; i < numOfCasts; i++) {
                    var pos = position + ((Vector3)size / 2) + interval * i;  
                    var testHit = Physics2D.Raycast(pos, castDirection, castWidth, layer);                    
                    if (testHit.collider) {
                        if (testHit.collider != null && testHit.distance <= hit.distance) {
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
#endif
        }
    }
}

