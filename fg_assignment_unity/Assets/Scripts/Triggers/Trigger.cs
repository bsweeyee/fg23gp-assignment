using Lander.GameState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Lander {
    public abstract class BoxTrigger : MonoBehaviour {
        [SerializeField] protected Vector3 offset;
        [SerializeField] protected Vector3 size;
        [SerializeField] protected Vector3 angle = Vector3.zero;
        protected UnityEvent<Collider2D, float> onEnterTrigger;
        protected UnityEvent<Collider2D, float> onTrigger;
        protected UnityEvent<Collider2D, float> onLeaveTrigger;
        protected Collider2D hit;               

        public void Initialize(Vector3 offset, Vector3 size, Vector3 angle) {
            onEnterTrigger = new UnityEvent<Collider2D, float>();
            onTrigger = new UnityEvent<Collider2D, float>();
            onLeaveTrigger = new UnityEvent<Collider2D, float>();

            if (offset.magnitude != 0) this.offset = offset;
            if (size.magnitude != 0) this.size = size;
            if (angle.magnitude != 0) this.angle = angle; 
        }

        public void ClearEvents() {
            onEnterTrigger.RemoveAllListeners();
            onTrigger.RemoveAllListeners();
            onLeaveTrigger.RemoveAllListeners();
        }

        public void OnTriggerCheck(LayerMask layer, float dt) {
            var direction = Quaternion.Euler(angle) * Vector3.up;
            var raycastHit = Physics2D.BoxCast(transform.position + (direction * offset.y), size, angle.z, direction.normalized, 0, layer);
            if (raycastHit.collider) {
                if (raycastHit != hit) {
                    onEnterTrigger?.Invoke(raycastHit.collider, dt);
                    if (hit != null) onLeaveTrigger?.Invoke(hit, dt);
                }

                onTrigger?.Invoke(raycastHit.collider, dt);
                hit = raycastHit.collider;
            }
            else if (hit != null) {
                onLeaveTrigger?.Invoke(hit, dt);
                hit = null;
            }            
        }

        #if UNITY_EDITOR
        protected virtual void OnDrawGizmos() {
            Gizmos.color = Color.green;
            var m = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.AngleAxis(angle.z, transform.forward), transform.localScale);
            Gizmos.DrawWireCube(offset, size);
            Gizmos.matrix = m;
        }
        #endif
    }
}
