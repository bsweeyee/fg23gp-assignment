using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Lander {
    public class WaterSpawner : MonoBehaviour
    {
        [SerializeField] private LayerMask layer;
        [SerializeField] private float spawnInterval = 1;

        PhysicsInteractorController controller;
        ParticleController particleController;
        float spawnTimer;        
        
        List<PhysicsInteractor> waterInteractors;
        List<PhysicsInteractor> waterTobeRemoved;

        public void Initialize(PhysicsInteractorController controller, ParticleController particleController) {
            this.controller = controller;
            this.particleController = particleController;
            waterInteractors = new List<PhysicsInteractor>();
            waterTobeRemoved = new List<PhysicsInteractor>();
        }

        public void Tick(Game game, float dt) {
            spawnTimer += dt;
            if (spawnTimer > spawnInterval) {
                CreateWater();
                spawnTimer = 0;
            }
            foreach(var interactor in waterInteractors) {
                interactor.OnTriggerCheck(layer);                
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

        public void DestroyWater(PhysicsInteractor water) {            
            water.Physics.Reset();
            waterTobeRemoved.Add(water);
            controller.WaterPool.Release(water);
        }

        #if UNITY_EDITOR

        void OnDrawGizmos() {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.4f);
        }

        #endif        
    }
}
