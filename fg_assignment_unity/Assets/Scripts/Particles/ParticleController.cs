using Lander.GameState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Lander {
    public class ParticleController : MonoBehaviour, ILevelPlayEntity
    {
        [SerializeField] private int maxPoolSize = 20;
        
        private Particle waterSplashPrefab;
        private Particle windParticlePrefab;

        private List<Particle> particleInstances;
        
        private List<Particle> particleMarkToDestroy;

        private IObjectPool<SplashParticle> waterSplashPool;        
        private IObjectPool<WindParticle> windPool;

        private IObjectPool<SplashParticle> WaterSplashPool {
            get {
                if (waterSplashPool == null) {
                    waterSplashPool = new LinkedPool<SplashParticle>(OnCreateParticle<SplashParticle>, OnTakeParticle<SplashParticle>, OnReturnParticle<SplashParticle>, OnReturnMaxParticle<SplashParticle>, true, maxPoolSize);                    
                }

                return waterSplashPool;
            }
        }
        
        private IObjectPool<WindParticle> WindPool {
            get {
                if (windPool == null) {
                    windPool = new LinkedPool<WindParticle>(OnCreateParticle<WindParticle>, OnTakeParticle<WindParticle>, OnReturnParticle<WindParticle>, OnReturnMaxParticle<WindParticle>, true, maxPoolSize);                    
                }

                return windPool;
            }
        }

        public bool IsEarlyInitialized { get; set; }

        public bool IsLateInitialized { get; set; }        

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
                pp = WindPool.Get();
            }

            if (pp != null) {
                pp.transform.position = position;
                particleInstances.Add(pp);
                pp.Initialize(this);            
            }            

            return pp;            
        }

        public void DestroyParticle(Particle pp) {            
            particleMarkToDestroy.Add(pp);            
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
                var splash = ws as SplashParticle;
                var wind = ws as WindParticle;
                if (splash != null) {
                    WaterSplashPool.Release(splash);
                } else if (wind != null) {
                    WindPool.Release(wind);
                } else {
                    Debug.LogError("Missing particle type");
                }                                    
            }
            particleMarkToDestroy.Clear();
        }

        void ILevelPlayEntity.OnFixedTick(Game game, float dt) {
        }        
    }
}
