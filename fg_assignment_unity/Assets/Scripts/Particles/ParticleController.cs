using Lander.GameState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Lander {
    public class ParticleController : MonoBehaviour, ILevelPlayEntity, ILevelPauseEntity, ILevelCompleteEntity, ILevelEndEntity
    {
        [SerializeField] private int maxPoolSize = 20;
        
        private Particle waterSplashPrefab;
        private Particle windParticlePrefab;

        private List<Particle> particleInstances;
        
        private List<Particle> particleMarkToDestroy;

        private IObjectPool<SplashParticle> waterSplashPool;        
        private IObjectPool<WindParticle> windGustPool;

        private IObjectPool<SplashParticle> WaterSplashPool {
            get {
                if (waterSplashPool == null) {
                    waterSplashPool = new LinkedPool<SplashParticle>(OnCreateParticle<SplashParticle>, OnTakeParticle<SplashParticle>, OnReturnParticle<SplashParticle>, OnReturnMaxParticle<SplashParticle>, true, maxPoolSize);                    
                }

                return waterSplashPool;
            }
        }
        
        private IObjectPool<WindParticle> WindGustPool {
            get {
                if (windGustPool == null) {
                    windGustPool = new LinkedPool<WindParticle>(OnCreateParticle<WindParticle>, OnTakeParticle<WindParticle>, OnReturnParticle<WindParticle>, OnReturnMaxParticle<WindParticle>, true, maxPoolSize);                    
                }

                return windGustPool;
            }
        }

        public bool IsEarlyInitialized { get; set; }

        public bool IsLateInitialized { get; set; }

        bool IGameInitializeEntity.IsEarlyInitialized => throw new System.NotImplementedException();

        bool IGameInitializeEntity.IsLateInitialized => throw new System.NotImplementedException();

        private T OnCreateParticle<T>() {
            var particle = default(T);
            if (typeof(T) == typeof(SplashParticle)) {
                particle = Instantiate(waterSplashPrefab).GetComponent<T>();            
            }
            else if (typeof(T) == typeof(WindParticle)) {
                particle = Instantiate(windParticlePrefab).GetComponent<T>();
            }

            return particle;
        }

        private void OnReturnParticle<T>(T particle) {
            var p = particle as MonoBehaviour;
            p.transform.position = Vector3.zero;
            p.gameObject.SetActive(false);            
        }

        private void OnTakeParticle<T>(T particle) {
            var p = particle as MonoBehaviour;
            p.gameObject.SetActive(true);
        }

        private void OnReturnMaxParticle<T>(T particle) {
            var p = particle as MonoBehaviour;
            Destroy(p.gameObject);
        }

        public virtual void EarlyInitialize(Game game) {
            if (IsEarlyInitialized) return;                               

            particleInstances = new List<Particle>();
            particleMarkToDestroy = new List<Particle>();
            
            IsEarlyInitialized = true;
        }

        public virtual void LateInitialize(Game game) {
            if (IsLateInitialized) return;

            IsLateInitialized = true;
        }

        public Particle CreateParticle<T>(Vector3 position) {
            Particle pp = null;
            if (typeof(T) == typeof(SplashParticle)) {
                pp = WaterSplashPool.Get();
            }
            else if (typeof(T) == typeof(WindParticle)) {
                pp = WindGustPool.Get();
            }

            if (pp != null) {
                pp.transform.position = position;
                particleInstances.Add(pp);
                pp.Initialize(this);            
            }            

            return pp;            
        }

        public void DestroyParticle(Particle pp) { 
            if (!particleMarkToDestroy.Contains(pp)) {
                particleMarkToDestroy.Add(pp);            
            }           
        }       

        void ILevelPlayEntity.OnEnter(Game game, IBaseGameState previous) {
            waterSplashPrefab = game.GameSettings.WaterSplashPrefab;
            windParticlePrefab = game.GameSettings.WindGustPrefab;
        }

        void ILevelPlayEntity.OnExit(Game game, IBaseGameState current) {
            
        }

        void ILevelPlayEntity.OnTick(Game game, float dt) {            
            foreach(var ws in particleInstances) {
                ws.Tick();
            }

            foreach(var ws in particleMarkToDestroy) {
                particleInstances.Remove(ws);
                if (!ws.gameObject.activeSelf) continue;
                var splash = ws as SplashParticle;
                var wind = ws as WindParticle;
                if (splash != null) {
                    WaterSplashPool.Release(splash);
                } else if (wind != null) {
                    WindGustPool.Release(wind);
                } else {
                    Debug.LogError("Missing particle type");
                }                                    
            }
            particleMarkToDestroy.Clear();
        }

        void ILevelPlayEntity.OnFixedTick(Game game, float dt) {
        }

        void ILevelPauseEntity.OnEnter(Game game, IBaseGameState previous) {
            foreach(var instance in particleInstances) {
                instance.Pause();
            }
        }

        void ILevelPauseEntity.OnExit(Game game, IBaseGameState current) {
            foreach(var instance in particleInstances) {
                instance.Play();
            }
        }

        void ILevelPauseEntity.OnTick(Game game, float dt) {
        }

        void ILevelPauseEntity.OnFixedTick(Game game, float dt) {
        }

        void ILevelCompleteEntity.OnEnter(Game game, IBaseGameState previous) {
            foreach(var ws in particleInstances) {
                var splash = ws as SplashParticle;
                var wind = ws as WindParticle;
                if (splash != null) {
                    WaterSplashPool.Release(splash);
                } else if (wind != null) {
                    WindGustPool.Release(wind);
                } else {
                    Debug.LogError("Missing particle type");
                }                                    
            }
            particleInstances.Clear();
            particleMarkToDestroy.Clear();
        }

        void ILevelCompleteEntity.OnExit(Game game, IBaseGameState current) {
        }

        void ILevelCompleteEntity.OnTick(Game game, float dt) {
        }

        void ILevelCompleteEntity.OnFixedTick(Game game, float dt) {
        }

        void ILevelEndEntity.OnEnter(Game game, IBaseGameState previous) {
            foreach(var ws in particleInstances) {
                var splash = ws as SplashParticle;
                var wind = ws as WindParticle;
                if (splash != null) {
                    WaterSplashPool.Release(splash);
                } else if (wind != null) {
                    WindGustPool.Release(wind);
                } else {
                    Debug.LogError("Missing particle type");
                }                                    
            }
            particleInstances.Clear();
            particleMarkToDestroy.Clear();
        }

        void ILevelEndEntity.OnExit(Game game, IBaseGameState current) {
        }

        void ILevelEndEntity.OnTick(Game game, float dt) {
        }

        void ILevelEndEntity.OnFixedTick(Game game, float dt) {
        }
    }
}
