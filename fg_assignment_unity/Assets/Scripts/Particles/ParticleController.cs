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

        private List<Particle> waterSplashInstances;
        private List<Particle> waterSplashMarkToDestroy;

        private IObjectPool<Particle> waterSplashPool;        

        public IObjectPool<Particle> WaterSplashPool {
            get {
                if (waterSplashPool == null) {
                    waterSplashPool = new LinkedPool<Particle>(OnCreateParticle, OnTakeParticle, OnReturnParticle, OnReturnMaxParticle, true, maxPoolSize);                    
                }

                return waterSplashPool;
            }
        }
        
        public bool IsEarlyInitialized { get; set; }

        public bool IsLateInitialized { get; set; }        

        private Particle OnCreateParticle() {
            var particle = Instantiate(waterSplashPrefab);            

            return particle;
        }

        private void OnReturnParticle(Particle particle) {
            particle.transform.position = Vector3.zero;
            particle.gameObject.SetActive(false);            
        }

        private void OnTakeParticle(Particle particle) {
            particle.gameObject.SetActive(true);
        }

        private void OnReturnMaxParticle(Particle particle) {
            Destroy(particle.gameObject);
        }

        public virtual void EarlyInitialize(Game game) {
            if (IsEarlyInitialized) return;                               

            waterSplashInstances = new List<Particle>();
            waterSplashMarkToDestroy = new List<Particle>();
            waterSplashPrefab = game.GameSettings.WaterSplashPrefab;
            
            IsEarlyInitialized = true;
        }

        public virtual void LateInitialize(Game game) {
            if (IsLateInitialized) return;

            IsLateInitialized = true;
        }

        public void CreateSplashParticle(Vector3 position) {
            var pp = WaterSplashPool.Get();
            pp.transform.position = position;
            waterSplashInstances.Add(pp);
            pp.Initialize(this);            
        }

        public void DestroySplashParticle(Particle pp) {
            waterSplashMarkToDestroy.Add(pp);            
        }       

        void ILevelPlayEntity.OnEnter(Game game, IBaseGameState previous) {
            waterSplashPrefab = game.GameSettings.WaterSplashPrefab;
        }

        void ILevelPlayEntity.OnExit(Game game, IBaseGameState current) {
            
        }

        void ILevelPlayEntity.OnTick(Game game, float dt) {            
            foreach(var ws in waterSplashInstances) {
                ws.Tick();
            }

            foreach(var ws in waterSplashMarkToDestroy) {
                waterSplashInstances.Remove(ws);
                WaterSplashPool.Release(ws);
            }
            waterSplashMarkToDestroy.Clear();
        }

        void ILevelPlayEntity.OnFixedTick(Game game, float dt) {
        }        
    }
}
