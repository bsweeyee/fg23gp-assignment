using Lander.Physics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using UnityEngine;

namespace Lander {
    public class WindInteractor : BoxTrigger
    {
        [SerializeField] private LayerMask layer;
        [SerializeField] private float strength = 1000;        
        [SerializeField] private AnimationCurve windDistanceFallOff;
        
        private float activeInterval;
        private float activeTimer;

        private WindSpawner spawner;

        public void Initialize(WindSpawner spawner, ParticleController pc) {
            base.Initialize(spawner.Offset, spawner.Size, spawner.Angle);
            this.spawner = spawner;
            this.activeInterval = spawner.WindActiveInterval;
            onTrigger.AddListener(OnApplyWind);            
        }
        
        public void Tick(float dt) {
            activeTimer += dt;
            if (activeTimer > activeInterval) {
                spawner.DestroyWind(this);
                activeTimer = 0;
                hit = null;
            }
            else {
                OnTriggerCheck(layer, dt);
            }            
        }

        void OnApplyWind(Collider2D collider, float dt) {
            var pc = collider.GetComponent<PhysicsController>();
            if (pc != null) {
                var diff = collider.transform.position - transform.position;
                var d = Quaternion.Euler(angle) * Vector3.up;
                var o = Vector3.Dot(diff, d.normalized) * d.normalized;
                var maxDistance = size.y;

                var s = strength * windDistanceFallOff.Evaluate(o.magnitude / maxDistance);                
                var direction = Quaternion.Euler(angle) * Vector3.up;
                pc.CurrentVelocity += direction * s * dt;
            }
        }        
    }
}
