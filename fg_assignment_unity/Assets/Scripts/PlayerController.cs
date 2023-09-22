using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
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
    private Vector3 inputDirection;
    private Vector3 currentVelocity;

    private BoxCollider2D collider;

    private bool isGrounded;
    private float controlDt;

    public void Start() {
        collider = GetComponent<BoxCollider2D>();
    }

    public void FixedUpdate() {
        CalculateVelocity(Time.fixedDeltaTime);        
        CalculateCollision();

        transform.position += currentVelocity;
    }

    private void CalculateVelocity(float dt) {

        var input = Vector3.zero;
        if (inputDirection.x < 0) {
            var targetDir = Vector3.Lerp(Vector3.up, -Vector3.right, targetFlightDirection).normalized;
            input = Vector3.Lerp(-Vector3.right, targetDir, lift.Evaluate(controlDt));
        } else if (inputDirection.x > 0) {
            var targetDir = Vector3.Lerp(Vector3.up, Vector3.right, targetFlightDirection).normalized;
            input = Vector3.Lerp(Vector3.right, targetDir, lift.Evaluate(controlDt));
        }

        // calculate acceleration
        var cA = Vector3.Lerp(controlAcceleration * 0.1f, controlAcceleration, jolt.Evaluate(controlDt));
        var finalAcceleration = new Vector3(cA.x * input.x, cA.y * input.y, cA.z * input.z);
        
        // calculate drag using formula 1/2*v^2*C
        var drag = 0.5f * currentVelocity.magnitude * currentVelocity.magnitude * dragCoefficient * currentVelocity.normalized;
        drag = (drag.y < 0.01f) ? new Vector3(drag.x, 0, drag.z) : drag;
        drag = (drag.magnitude < 0.001f) ? Vector3.zero : drag;
        
        finalAcceleration = finalAcceleration - drag;
        currentVelocity += finalAcceleration * dt;

        // add gravity
        if(!isGrounded && currentVelocity.y < Mathf.Abs(maximumVelocity.y)) {
            currentVelocity += gravity;
        }

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
        if (currentVelocity.y > 0) isGrounded = false;
        if(isGrounded) {
            currentVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        }
        else
        {
            if (currentVelocity.y < 0)
            {
                // downward collision raycast
                var size = collider.size;
                var widthDiff = size.x / numOfCasts;

                for(int i = 0; i < numOfCasts; i++) {
                    var pos = transform.position - (Vector3)collider.size / 2 + new Vector3(widthDiff * i, 0, 0);
                    var hitInfos = Physics2D.RaycastAll(pos, Vector3.down, raycastSkinWidth);
                    foreach(var hit in hitInfos) {
                        if(hit.collider.gameObject == gameObject) continue;
                        var diff = Vector3.Dot(((Vector3)hit.point - pos), Vector3.down) * Vector3.down;
                        currentVelocity = new Vector3(currentVelocity.x, -diff.magnitude, currentVelocity.z);
                        if(diff.magnitude > 0.01f) isGrounded = true;
                        break;
                    }
                    if (hitInfos.Length <= 0) isGrounded = false;
                }
            }            

            // TODO: side and top collision raycast
        }

        // if (currentVelocity.y > 0) isGrounded = false;
        if (currentVelocity.magnitude < 0.01f) currentVelocity = Vector3.zero;
    }

    // TODO: move this to a input controller script to map buttons different depending on game state
    public void OnInputPress(InputAction.CallbackContext context) {
        OnDirectionIO(context.ReadValue<Vector2>());
    }

    public void OnDirectionIO(Vector3 dir) {
        // TODO: might want to change input dir to 1 instead of normalized
        inputDirection = dir;    
    }
 
#if UNITY_EDITOR
    private void OnDrawGizmos() {
        if(collider == null) collider = GetComponent<BoxCollider2D>();
        var size = collider.size;
        var widthDiff = size.x / (numOfCasts-1);

        // downward casts
        for(int i = 0; i < numOfCasts; i++) {
            var pos = transform.position - (Vector3)size / 2 + new Vector3(widthDiff * i, 0, 0);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(pos, pos + (Vector3.down * raycastSkinWidth));
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
