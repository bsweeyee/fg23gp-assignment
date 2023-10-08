using Lander.GameState;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Lander {
    public class InteractorController : MonoBehaviour, ILevelPlayEntity
    {   
        [SerializeField] private int maxPoolSize = 20;

        private WaterDropletInteractor waterPrefab;
        private WindInteractor windPrefab;

        private IObjectPool<WaterDropletInteractor> waterPool;
        private IObjectPool<WindInteractor> windPool;

        private WaterSpawner[] waterSpawners;    

        private WindSpawner[] windSpawners;

        public IObjectPool<WaterDropletInteractor> WaterPool {
            get {
                if (waterPool == null) {
                    waterPool = new LinkedPool<WaterDropletInteractor>(OnCreate<WaterDropletInteractor>, OnTake<WaterDropletInteractor>, OnReturn<WaterDropletInteractor>, OnReturnMax<WaterDropletInteractor>, true, maxPoolSize);                    
                }

                return waterPool;
            }
        }

        public IObjectPool<WindInteractor> WindPool {
             get {
                if (windPool == null) {
                    windPool = new LinkedPool<WindInteractor>(OnCreate<WindInteractor>, OnTake<WindInteractor>, OnReturn<WindInteractor>, OnReturnMax<WindInteractor>, true, maxPoolSize);                    
                }

                return windPool;
            }
        }

        public bool IsEarlyInitialized { get; set; }

        public bool IsLateInitialized { get; set; }

        public virtual void EarlyInitialize(Game game) {
            if (IsEarlyInitialized) return;

            waterPrefab = game.GameSettings.WaterPrefab;
            windPrefab = game.GameSettings.WindPrefab;            
            
            Prespawn<WaterDropletInteractor>();
            Prespawn<WindInteractor>();                                  

            IsEarlyInitialized = true;
        }

        public virtual void LateInitialize(Game game) {
            if (IsLateInitialized) return;

            IsLateInitialized = true;
        }

        private void Prespawn<T>() {                        
            if (typeof(T) == typeof(WaterDropletInteractor)) {
                var wList = new List<WaterDropletInteractor>();
                for(int i=0; i<maxPoolSize; i++) {
                    var water = WaterPool.Get();
                    wList.Add(water);
                }
                foreach(var w in wList) {
                    WaterPool.Release(w);
                }    
            } else if (typeof(T) == typeof(WindInteractor)) {
                var wList = new List<WindInteractor>();
                for(int i=0; i<maxPoolSize; i++) {
                    var wind = WindPool.Get();
                    wList.Add(wind);
                }
                foreach(var w in wList) {
                    WindPool.Release(w);
                } 
            }             
        }

        private T OnCreate<T>() {
            T item = default(T);
            if (typeof(T) == typeof(WaterDropletInteractor)) {
                item = Instantiate(waterPrefab).GetComponent<T>();
            } else if (typeof(T) == typeof(WindInteractor)) {
                item = Instantiate(windPrefab).GetComponent<T>();
            }
                        
            return item;
        }         

        private void OnReturn<T>(T item) {
            var o = item as MonoBehaviour;
            o.transform.position = Vector3.zero;
            o.gameObject.SetActive(false);            
        }

        private void OnTake<T>(T item) {
            var o = item as MonoBehaviour;
            o.gameObject.SetActive(true);
        }

        private void OnReturnMax<T>(T item) {
            var o = item as MonoBehaviour;
            Destroy(o.gameObject);
        }        

        void ILevelPlayEntity.OnEnter(Game game, IBaseGameState previous) {
            if (previous.GetType() == typeof(PauseState)) return;
            
            waterSpawners = FindObjectsOfType<WaterSpawner>().ToArray();                                
            foreach(var spawner in waterSpawners) {
                spawner.Initialize(this, game.ParticleController);
            }

            windSpawners = FindObjectsOfType<WindSpawner>().ToArray();
            foreach(var wi in windSpawners) {
                wi.Initialize(this, game.ParticleController);
            }
        }

        void ILevelPlayEntity.OnExit(Game game, IBaseGameState current) {
        }

        void ILevelPlayEntity.OnTick(Game game, float dt) {            
            foreach(var spawner in waterSpawners) {
                spawner.Tick(game, dt);
            }

            foreach(var spawner in windSpawners) {
                spawner.Tick(game, dt);              
            } 
        }

        void ILevelPlayEntity.OnFixedTick(Game game, float dt) {
        }        
    }
}

