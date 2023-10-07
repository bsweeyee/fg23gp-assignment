using Lander.Physics;
using UnityEngine;
using UnityEngine.Events;

namespace Lander {
    public class PhysicsInteractor : BoxTrigger {
        [SerializeField] private LayerMask layer;
        [SerializeField] private Vector3 gravity;
        [SerializeField] private float strength = 2000;
        [SerializeField] private ParticleSystem particle;

        private PhysicsController physics;
        private WaterSpawner spawner;

        public PhysicsController Physics {
            get {
                return physics;
            }
        }        

        public void Initialize(WaterSpawner spawner, ParticleController particleController) {
            physics = GetComponent<PhysicsController>();
            this.spawner = spawner; 
            physics.Gravity = gravity;
            physics.Layer = layer;
            onTrigger = new UnityEvent<Collider2D>();
            onTrigger.AddListener( (Collider2D collider) => {
                var pc = collider.GetComponent<PhysicsController>();                
                if (pc != null) {
                    var pDirection = pc.CurrentVelocity.normalized;
                    if (pc.CurrentVelocity.y < 0) {
                        var sign = Vector3.Dot((collider.transform.position - transform.position), Vector3.right);
                        pDirection = Vector3.Reflect(pDirection, new Vector3(0, Mathf.Sign(sign), 0));
                    }
                    
                    var force = Vector3.Lerp(-pDirection, physics.CurrentVelocity.normalized, 0.5f);
                    pc.AddAcceleration(force * strength);
                } 

                particleController.CreateSplashParticle(transform.position);                                                                
                spawner.DestroyWater(this);                
            });
        }
    }
}
