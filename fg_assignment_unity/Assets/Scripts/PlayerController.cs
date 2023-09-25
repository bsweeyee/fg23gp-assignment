using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

    [SerializeField][Range(0, 1)] private float targetFlightDirection;
    [SerializeField] private AnimationCurve jolt;
    [SerializeField] private AnimationCurve lift;
    [SerializeField] private Vector3 controlAcceleration;
    [SerializeField] private Vector3 gravity;
    [SerializeField][Min(0.1f)] private float dragCoefficient = 5;

    [SerializeField] private Vector3 maximumVelocity;

    [SerializeField][Min(2)] private int numOfCasts = 3;
    [SerializeField] private float raycastSkinWidth = 1;
    
    private Vector3 previousVelocity;
    private Vector3 currentVelocity;

    private BoxCollider2D collider;

    private Vector3 inputDirection;
    private bool isGrounded;
    private float controlDt;

    public void Start() {
        collider = GetComponent<BoxCollider2D>();
    }

    public void FixedUpdate() {
        previousVelocity = currentVelocity;

        CalculateVelocity(Time.fixedDeltaTime);        
        CalculateCollision();

        transform.position += currentVelocity;
        if (isGrounded) currentVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
    }

    private void CalculateVelocity(float dt) {
        
        // modify input controls
        var input = Vector3.zero;
        if (inputDirection.x < 0) {
            var targetDir = Vector3.Lerp(Vector3.up, -Vector3.right, targetFlightDirection).normalized;
            input = Vector3.Lerp(-Vector3.right, targetDir, lift.Evaluate(controlDt));
        } else if (inputDirection.x > 0) {
            var targetDir = Vector3.Lerp(Vector3.up, Vector3.right, targetFlightDirection).normalized;
            input = Vector3.Lerp(Vector3.right, targetDir, lift.Evaluate(controlDt));
        }
        
        // calculate drag using formula 1/2*v^2*C
        var drag = 0.5f * currentVelocity.magnitude * currentVelocity.magnitude * dragCoefficient * currentVelocity.normalized;
        drag = (drag.y < 0.01f) ? new Vector3(drag.x, 0, drag.z) : drag;
        drag = (drag.magnitude < 0.001f) ? Vector3.zero : drag;
        
        // calculate acceleration
        var cA = Vector3.Lerp(controlAcceleration * 0.1f, controlAcceleration, jolt.Evaluate(controlDt));
        var finalAcceleration = new Vector3(cA.x * input.x, cA.y * input.y, cA.z * input.z); 
        finalAcceleration = finalAcceleration - drag;
        if(!isGrounded && Mathf.Abs(currentVelocity.y) < maximumVelocity.y) {
            finalAcceleration += gravity;
        }

        currentVelocity += (finalAcceleration * dt);

        // add to control dt
        if (inputDirection.magnitude > 0) {
            controlDt += dt;
            controlDt = Mathf.Clamp01(controlDt);
        }
        else {
            controlDt = 0;
        }
    }

    private void CalculateCollision() {
        if ( Mathf.Abs(currentVelocity.x) <= 0.01f) currentVelocity = new Vector3(0, currentVelocity.y, currentVelocity.z);
        
        var size = collider.size;
        var heightDiff = size.y / numOfCasts;    
        var widthDiff = size.x / numOfCasts;                            
        var hitDifference = new List<Tuple<Vector3, Vector3>>();

        // raycast in direction of current x velocity direction
        var xSign = (currentVelocity.x == 0) ? 0 : Mathf.Sign(currentVelocity.x); 
        hitDifference = Cast(new Vector3(0, heightDiff, 0), new Vector3(xSign * size.x, -size.y, 0) , xSign * Vector3.right);
        if (hitDifference.Count > 0) {            
            hitDifference = hitDifference.OrderBy( x => (x.Item1 - x.Item2).magnitude ).ToList();
            if (hitDifference.Count > 0 && (hitDifference[0].Item1 - hitDifference[0].Item2).magnitude <= 0.01f) {
                var mainPosition = hitDifference[0].Item2 - xSign * new Vector3(size.x / 2, 0, 0);
                RaycastHit2D hitPos;
                bool isHit = GetCollisionPoint(mainPosition, size.x, xSign * Vector3.right, out hitPos);
                
                if (isHit && !isGrounded) {
                    currentVelocity = new Vector3( -xSign * ((size.x / 2) - xSign * (hitPos.point.x - mainPosition.x) + 0.001f), currentVelocity.y, currentVelocity.z);
                }                                                                                           
            }          
        }

        // raycast to check if anything in the y direction is hit and find the cast that is has the shortest distance 
        var ySign = (currentVelocity.y == 0) ? 0 : Mathf.Sign(currentVelocity.y);
        hitDifference.Clear();
        hitDifference = Cast(new Vector3(widthDiff, 0, 0), new Vector3(-size.x, ySign == 0? -1 : ySign * size.y, 0), ySign * Vector3.up);

        if (hitDifference.Count > 0) { 
            hitDifference = hitDifference.OrderBy( x => (x.Item1 - x.Item2).magnitude ).ToList();                       
                   
            if ((hitDifference[0].Item1 - hitDifference[0].Item2).magnitude <= 0.01f) {
                var mainPosition = hitDifference[0].Item2 - ySign * new Vector3(0, size.y / 2, 0);
                RaycastHit2D hitPos;
                var isHit = GetCollisionPoint(mainPosition, size.y, ySign * Vector3.up, out hitPos);
                // if (Vector3.Dot(hitPos.collider.transform.up, -ySign * Vector3.up) < 0.01) return;
                
                if (isHit) {
                    if (ySign < 0) currentVelocity = new Vector3(0, (size.y / 2) - ySign * (hitPos.point.y - mainPosition.y), currentVelocity.z);                                                                    
                    else if (ySign > 0) currentVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
                }

                if (ySign < 0) { Debug.Log(isHit); isGrounded = true; return; }
                else if (ySign > 0) { isGrounded = false; } 
            }                                                 
        }
        else {            
            isGrounded = false;
        }                    

        if (currentVelocity.y > 0) isGrounded = false;
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

    // TODO: move this to a input controller script to map buttons different depending on game state
    public void OnInputPress(InputAction.CallbackContext context) {
        OnDirectionIO(context.ReadValue<Vector2>());
    }

    public void OnDirectionIO(Vector3 dir) {
        // TODO: might want to change input dir to 1 instead of normalized
        if (inputDirection.x != dir.x) controlDt = 0;
        inputDirection = dir;    
    }
 
#if UNITY_EDITOR
    private void OnDrawGizmos() {
        if(collider == null) collider = GetComponent<BoxCollider2D>();
        var size = collider.size;
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

        // draw target velocity
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + ( Vector3.Lerp(Vector3.up, Vector3.right, targetFlightDirection).normalized * 0.5f ));
        Gizmos.DrawLine(transform.position, transform.position + ( Vector3.Lerp(Vector3.up, -Vector3.right, targetFlightDirection).normalized * 0.5f ));
    }
#endif
}
