using Lander.GameState;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using UnityEngine;

namespace Lander {
    public class CameraController : MonoBehaviour, IEntities {
        [SerializeField] private float cameraSpeed = 0.5f;
        [SerializeField] private float maxCameraTargetDistance = 5;
        private Transform followTarget;

        public Transform FollowTarget {
            set {
                followTarget = value;
            }
        }

         public void Initialize(Game game) {
            var players = game.Entities.Where( x=> x.GetType() == typeof(Player) ).ToArray();            
            if (players.Length > 0 ) {
                followTarget = (players[0] as Player).transform;
            }
        }

        public void OnFixedTick(Game game, float dt) {
            if (game.CurrentState == Game.PLAY_STATE) {
                if (followTarget != null) {
                    var plane = new Plane(transform.forward, transform.position);
                    var targetPosition = plane.ClosestPointOnPlane(followTarget.position);                
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

        public void OnTick(Game game, float dt) {
        }        

        public void OnEnter(Game game, IBaseGameState previous, IBaseGameState current) {
        }

        public void OnExit(Game game, IBaseGameState previous, IBaseGameState current) {
        }        
    }
}
