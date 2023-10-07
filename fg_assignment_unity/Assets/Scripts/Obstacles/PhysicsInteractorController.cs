using Lander.GameState;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Lander {
    public class PhysicsInteractorController : MonoBehaviour, ILevelPlayEntity
    {   
        [SerializeField] private int maxPoolSize = 20;

        private PhysicsInteractor waterPrefab;

        private IObjectPool<PhysicsInteractor> waterPool;

        private WaterSpawner[] spawners;    

        public IObjectPool<PhysicsInteractor> WaterPool {
            get {
                if (waterPool == null) {
                    waterPool = new LinkedPool<PhysicsInteractor>(OnCreateWater, OnTakeWater, OnReturnWater, OnReturnMaxWater, true, maxPoolSize);                    
                }

                return waterPool;
            }
        }

        public bool IsEarlyInitialized { get; set; }

        public bool IsLateInitialized { get; set; }

        public virtual void EarlyInitialize(Game game) {
            if (IsEarlyInitialized) return;

            waterPrefab = game.GameSettings.WaterPrefab;            
            
            var wList = new List<PhysicsInteractor>();
            for(int i=0; i<maxPoolSize; i++) {
                var water = WaterPool.Get();
                wList.Add(water);
            }
            foreach(var w in wList) {
                WaterPool.Release(w);
            }
                        

            IsEarlyInitialized = true;
        }

        public virtual void LateInitialize(Game game) {
            if (IsLateInitialized) return;

            IsLateInitialized = true;
        } 

        private PhysicsInteractor OnCreateWater() {
            var water = Instantiate(waterPrefab);            

            return water;
        }

        private void OnReturnWater(PhysicsInteractor water) {
            water.transform.position = Vector3.zero;
            water.gameObject.SetActive(false);            
        }

        private void OnTakeWater(PhysicsInteractor water) {
            water.gameObject.SetActive(true);
        }

        private void OnReturnMaxWater(PhysicsInteractor water) {
            Destroy(water.gameObject);
        }

        void ILevelPlayEntity.OnEnter(Game game, IBaseGameState previous) {
            spawners = FindObjectsOfType<WaterSpawner>().ToArray();                                
            foreach(var spawner in spawners) {
                spawner.Initialize(this, game.ParticleController);
            }
        }

        void ILevelPlayEntity.OnExit(Game game, IBaseGameState current) {
        }

        void ILevelPlayEntity.OnTick(Game game, float dt) {            
            foreach(var spawner in spawners) {
                spawner.Tick(game, dt);
            }
        }

        void ILevelPlayEntity.OnFixedTick(Game game, float dt) {
        }        
    }
}

