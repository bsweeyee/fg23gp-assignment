using Lander.GameState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Lander {
    public abstract class BoxTrigger : MonoBehaviour {
        [SerializeField] private Vector3 size;
        protected UnityEvent<Collider2D> onTrigger;               

        public void OnTriggerCheck(LayerMask layer) {
            var hitCollider = Physics2D.OverlapBox(transform.position, size, 0, layer);
            if (hitCollider) {
                onTrigger?.Invoke(hitCollider);
            }
        }

        #if UNITY_EDITOR
        void OnDrawGizmos() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, size);
        }
        #endif
    }
}
