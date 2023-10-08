using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Lander {
    public class WaterSpawner : MonoBehaviour
    {
        [SerializeField] private LayerMask layer;
        [SerializeField] private float spawnInterval = 1;
        [SerializeField] private Vector3 size;

        InteractorController controller;
        ParticleController particleController;
        float spawnTimer;        
        
        List<WaterDropletInteractor> waterInteractors;
        List<WaterDropletInteractor> waterTobeRemoved;

        public Vector3 Size {
            get { return size; }
        }

        public void Initialize(InteractorController controller, ParticleController particleController) {
            this.controller = controller;
            this.particleController = particleController;
            waterInteractors = new List<WaterDropletInteractor>();
            waterTobeRemoved = new List<WaterDropletInteractor>();
        }

        public void Tick(Game game, float dt) {
            spawnTimer += dt;
            if (spawnTimer > spawnInterval) {
                CreateWater();
                spawnTimer = 0;
            }
            foreach(var interactor in waterInteractors) {
                interactor.OnTriggerCheck(layer, dt);                
            }

            foreach(var r in waterTobeRemoved) {
                waterInteractors.Remove(r);
            }
            waterTobeRemoved.Clear();
        }

        public void CreateWater() {
            var waterInteractor = controller.WaterPool.Get();
            waterInteractor.transform.position = transform.position;
            waterInteractors.Add(waterInteractor);
            waterInteractor.Initialize(this, particleController);
        }

        public void DestroyWater(WaterDropletInteractor water) {            
            water.Physics.Reset();
            water.ClearEvents();
            waterTobeRemoved.Add(water);
            controller.WaterPool.Release(water);
        }

        void OnDestroy() {
            foreach (var water in waterInteractors) {
                particleController.DestroyParticle(water.ParticleInstance);
                water.Physics.Reset();
                water.ClearEvents();
                controller.WaterPool.Release(water);                
            }

            foreach (var water in waterTobeRemoved) {
                particleController.DestroyParticle(water.ParticleInstance);
                water.Physics.Reset();
                water.ClearEvents();
                controller.WaterPool.Release(water);                
            }


            waterInteractors.Clear();
            waterTobeRemoved.Clear();        
        }

        #if UNITY_EDITOR

        void OnDrawGizmos() {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.4f);
        }

        #endif        
    }
}
