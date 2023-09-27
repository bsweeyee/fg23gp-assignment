using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Linq;

namespace Lander {
    public class Game : MonoBehaviour
    {
        public static Game instance;
        private InputController inputController;
        private IPhysics[] physics;
        private IEntities[] entities;

        public IEntities[] Entities {
            get { return entities; }
        }

        public void Initialize() {
            if (instance == null) {
                instance = this;
                DontDestroyOnLoad(gameObject);
                
                InitInput();
                InitEntities();
                InitPhysics();

                return;
            }

            Destroy(gameObject);
        }

        void InitInput() {
            inputController = FindObjectOfType<InputController>();
            inputController.Initialize();
        }

        void InitEntities() {
            entities = FindObjectsOfType<MonoBehaviour>().OfType<IEntities>().ToArray();
            foreach(var entity in entities) {
                entity.Initialize(this);
            }
        }

        void InitPhysics() {
            physics = FindObjectsOfType<MonoBehaviour>().OfType<IPhysics>().ToArray();
            foreach(var p in physics) {
                p.Initialize();
            }
        }

        void Update() {
            var dt = Time.deltaTime;

            foreach(var entity in entities) {
                entity.Tick(dt);
            }

            foreach(var p in physics) {
                p.OnTick(dt);
            }
        }

        void FixedUpdate() {
            var dt = Time.fixedDeltaTime;

            foreach(var entity in entities) {
                entity.FixedTick(dt);
            }

            foreach(var p in physics) {
                p.OnFixedTick(dt);
            }
        }
    }
}

