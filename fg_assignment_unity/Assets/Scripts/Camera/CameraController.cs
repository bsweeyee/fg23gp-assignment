using Lander.GameState;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using UnityEngine;

namespace Lander {
    public class CameraController : MonoBehaviour, ILevelStartEntity, ILevelPlayEntity {
        [SerializeField] private float cameraSpeed = 0.5f;
        [SerializeField] private float maxCameraTargetDistance = 5;
        private Transform followTarget;
        private Camera gameCamera;

        public Transform FollowTarget {
            set {
                followTarget = value;
            }
        }

        public Vector3 TargetPosition {
            get {
                var targetPosition = Vector3.zero;
                if (followTarget != null) {
                    var plane = new Plane(transform.forward, transform.position);
                    targetPosition = plane.ClosestPointOnPlane(followTarget.position); 
                    targetPosition.z = transform.position.z;
                }
                return targetPosition;
            }
        }

        public Camera Camera {
            get {
                return this.gameCamera;
            }
        }

        public bool IsEarlyInitialized { get; private set; }

        public bool IsLateInitialized { get; private set; }

        bool IGameInitializeEntity.IsEarlyInitialized => throw new System.NotImplementedException();

        bool IGameInitializeEntity.IsLateInitialized => throw new System.NotImplementedException();

        public void EarlyInitialize(Game game) {
            if (IsEarlyInitialized) return;

            this.gameCamera = GetComponent<Camera>();

            IsEarlyInitialized = true;
        }

        public void LateInitialize(Game game) { 
            if (IsLateInitialized) return;       
        
            IsLateInitialized = true;
        }

        void ILevelPlayEntity.OnFixedTick(Game game, float dt) {
            if (game.CurrentState == Game.PLAY_STATE) {
                if (followTarget != null) {
                    var targetPosition = TargetPosition;                
                    var moveDirection = (targetPosition - transform.position);
                                                    
                    var newPosition = transform.position + (Mathf.InverseLerp(0, maxCameraTargetDistance, moveDirection.magnitude) * cameraSpeed * dt * moveDirection.normalized);
                    if ((targetPosition - transform.position).magnitude > 0.1) {                    
                        // transform.position = Vector3.Lerp(transform.position, newPosition, 0.5f);
                        transform.position = newPosition;
                    }
                    else {
                        transform.position = targetPosition;
                    }
                }        
            }
        }

        void ILevelPlayEntity.OnTick(Game game, float dt) {
        }        

        void ILevelPlayEntity.OnEnter(Game game, IBaseGameState previous) {
        }

        void ILevelPlayEntity.OnExit(Game game, IBaseGameState previous) {
        }

        void ILevelStartEntity.OnEnter(Game game, IBaseGameState previous) {
        }

        void ILevelStartEntity.OnExit(Game game, IBaseGameState current) {
        }

        void ILevelStartEntity.OnTick(Game game, float dt) {
        }

        void ILevelStartEntity.OnFixedTick(Game game, float dt) {
        }
    }
}
