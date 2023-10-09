using Lander.Physics;
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
        private ParticleController pc;
        private Particle particleInstance;        

        public void Initialize(WindSpawner spawner, ParticleController pc) {
            base.Initialize();
            
            offset = spawner.Offset;
            size = spawner.Size;
            angle = spawner.Angle;
            strength = spawner.Strength;

            this.spawner = spawner;
            this.activeInterval = spawner.WindActiveInterval;
            onTrigger.AddListener(OnApplyWind);
            // if (particleInstance == null) {                                                        
            //     Debug.Log(GetInstanceID() + ": " + particleInstance.GetInstanceID()); 
            // } else {
            //     particleInstance.transform.position = spawner.transform.position;
            //     particleInstance.Play();
            // }

            particleInstance = pc.CreateParticle<WindParticle>(spawner.transform.position) as WindParticle;
            var d = Quaternion.Euler(angle) * Vector3.up;
            particleInstance.transform.up = d;            
            this.pc = pc;
        }
        
        public void Tick(float dt) {
            if (activeInterval > 0) {
                activeTimer += dt;
                if (activeTimer > activeInterval) {
                    // pc.DestroyParticle(particle);
                    particleInstance.Stop();                
                    spawner.DestroyWind(this);
                    activeTimer = 0;
                    hit = null;
                }                            
            }            
            OnTriggerCheck(layer, dt);
        }

        void OnApplyWind(Collider2D collider, float dt) {
            var pc = collider.GetComponent<PhysicsController>();
            if (pc != null) {
                var diff = collider.transform.position - transform.position;
                var d = Quaternion.Euler(angle) * Vector3.up;
                var o = Vector3.Dot(diff, d.normalized) * d.normalized;
                var maxDistance = size.y;

                var s = strength * windDistanceFallOff.Evaluate(o.magnitude / maxDistance);                                
                pc.CurrentVelocity += d * s * dt;
            }
        }                       
    }
}
