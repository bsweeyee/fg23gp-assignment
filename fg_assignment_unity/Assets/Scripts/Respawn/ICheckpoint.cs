using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace Lander {
    public abstract class Checkpoint : MonoBehaviour {
        [SerializeField] protected Vector3 spawnLocalPosition;

        public static Vector3 CurrentSpawnWorldPosition {
            get { return currentSpawnWorldPosition; }
            set {
                currentSpawnWorldPosition = value;
            }
        }

        private static Vector3 currentSpawnWorldPosition;

        public virtual Vector3 SpawnWorldPosition {
            get {
                return transform.TransformPoint(spawnLocalPosition);
            }
        }

        public static void Respawn(Transform body) {
            body.position = CurrentSpawnWorldPosition;
        }

        #if UNITY_EDITOR

        protected virtual void OnDrawGizmos() {
            Gizmos.color = (CurrentSpawnWorldPosition == SpawnWorldPosition) ? Color.green : Color.red;
            if (SpawnWorldPosition != Vector3.zero) {
                Gizmos.DrawWireSphere(transform.TransformPoint(spawnLocalPosition), 0.5f);
            }
        }

        #endif
    }
}
