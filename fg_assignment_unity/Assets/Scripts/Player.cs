using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {

    [SerializeField] private Vector3 controlAcceleration;
    [SerializeField] private Vector3 gravity;
    [SerializeField] private Vector3 drag;

    [SerializeField] private Vector3 maximumVelocity;

    [SerializeField][Min(2)] private int numOfCasts = 3;
    [SerializeField] private float raycastSkinWidth = 1;
    private Vector3 inputDirection;
    private Vector3 currentVelocity;

    private BoxCollider2D collider;

    private bool isGrounded;

    public void Start() {
        collider = GetComponent<BoxCollider2D>();
    }

    public void FixedUpdate() {
        Move(Time.fixedDeltaTime);
        CheckCollision();

        transform.position += currentVelocity;
    }

    private void Move(float dt) {

        // add input acceleration
        var a = new Vector3(controlAcceleration.x * inputDirection.x, controlAcceleration.y * inputDirection.y, controlAcceleration.z * inputDirection.z);
        currentVelocity += a * dt;

        // add gravity
        if(!isGrounded && currentVelocity.y < Mathf.Abs(maximumVelocity.y)) {
            currentVelocity += gravity;
        }
    }

    private void CheckCollision() {
        // TODO: use raycasting to check next position. move by difference if hit a point and set y velocity to 0
        if(isGrounded) {
            currentVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.y);
        }

        // downward casts
        if(!isGrounded) {
            var size = collider.size;
            var widthDiff = size.x / numOfCasts;

            for(int i = 0; i < numOfCasts; i++) {
                var pos = transform.position - (Vector3)collider.size / 2 + new Vector3(widthDiff * i, 0, 0);
                var hitInfos = Physics2D.RaycastAll(pos, Vector3.down, raycastSkinWidth);
                foreach(var hit in hitInfos) {
                    if(hit.collider.gameObject == gameObject) continue;
                    // TODO: move by difference between hit position and current position, then set to grounded
                    var diff = Vector3.Dot((pos - (Vector3)hit.point), (pos + (Vector3.down * raycastSkinWidth))) * Vector3.down;
                    currentVelocity = new Vector3(currentVelocity.x, diff.magnitude, currentVelocity.z);
                    isGrounded = true;
                    break;
                }
            }
        }

        if(currentVelocity.y > 0) {
            isGrounded = false;
        }
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
    }
#endif
}
