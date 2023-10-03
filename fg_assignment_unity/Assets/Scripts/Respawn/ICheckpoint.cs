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
                if (value != Vector3.zero) {
                    currentSpawnWorldPosition = value;
                }
            }
        }

        private static Vector3 currentSpawnWorldPosition;

        public Vector3 SpawnWorldPosition {
            get {
                if (spawnLocalPosition == Vector3.zero) return Vector3.zero;
                return transform.TransformPoint(spawnLocalPosition);
            }
        }

        public void SetLocalSpawnPoint(Vector3 spawnPointLocal) {
            spawnLocalPosition = spawnPointLocal;
        }

        public static void Respawn(Transform body) {
            body.position = CurrentSpawnWorldPosition;
        }

        #if UNITY_EDITOR

        private void OnDrawGizmos() {
            Gizmos.color = (CurrentSpawnWorldPosition == SpawnWorldPosition) ? Color.green : Color.red;
            if (SpawnWorldPosition != Vector3.zero) {
                Gizmos.DrawWireSphere(transform.TransformPoint(spawnLocalPosition), 0.5f);
            }
        }

        #endif
    }
}
