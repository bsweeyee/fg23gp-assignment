using Lander.Physics;
using UnityEngine;
using UnityEngine.Events;

namespace Lander {
    public class WaterDropletInteractor : BoxTrigger {
        [SerializeField] private LayerMask layer;
        [SerializeField] private Vector3 gravity;
        [SerializeField] private float strength = 2000;
        
        private SplashParticle particleInstance;

        private PhysicsController physics;

        public PhysicsController Physics {
            get {
                return physics;
            }
        }

        public SplashParticle ParticleInstance {
            get {
                return particleInstance;
            }
        }        

        public void Initialize(WaterSpawner spawner, ParticleController particleController) {
            physics = GetComponent<PhysicsController>();
            physics.Gravity = gravity;
            physics.Layer = layer;
            base.Initialize(Vector3.zero, spawner.Size, Vector3.zero);
            onEnterTrigger.AddListener( (Collider2D collider, float dt) => {
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

                particleInstance = particleController.CreateParticle<SplashParticle>(transform.position) as SplashParticle;                                                                
                spawner.DestroyWater(this);                
            });
        }         
    }
}
