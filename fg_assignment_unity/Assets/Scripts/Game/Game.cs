using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Linq;
using UnityEditor;
using Lander.GameState;

namespace Lander {
    public class Game : MonoBehaviour, IDebug
    {
        public static PlayState PLAY_STATE;
        public static StartState START_STATE;
        public static DeathState DEATH_STATE;

        public static Game instance;

        [SerializeField][Range(0, 1)] private float timeFactor = 1;

        private InputController inputController;
        private IGameStateEntity[] entities;
        private IPhysics[] physics;
        private IDebug[] debugs;
        private BaseGameState currentState;

        public BaseGameState CurrentState {
            get { return currentState; }
            set {
                var previousState = currentState;
                currentState = value;

                if (currentState != null) {
                    currentState.OnEnter(this, previousState, currentState);
                    currentState.OnExit(this, previousState, currentState);
                }
            }
        }

        public IGameStateEntity[] Entities {
            get { return entities; }
        }

        public void Initialize() {
            if (instance == null) {
                instance = this;
                DontDestroyOnLoad(gameObject);

                EarlyInitialize();
                LateInitialize();

                return;
            }

            Destroy(gameObject);
        }       

        void EarlyInitialize() {
            inputController = FindObjectOfType<InputController>();
            inputController?.EarlyInitialize(this);

            physics = FindObjectsOfType<MonoBehaviour>().OfType<IPhysics>().ToArray();
            foreach(var p in physics) {
                p.EarlyInitialize(this);
            }

            entities = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IGameStateEntity>().ToArray();

            START_STATE = new StartState();
            PLAY_STATE = new PlayState();
            DEATH_STATE = new DeathState();

            currentState = PLAY_STATE;
            currentState.EarlyInitialize(this);

            debugs = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IDebug>().ToArray();
        }

        void LateInitialize() {
            inputController?.LateInitialize(this);
            foreach(var p in physics) {
                p.LateInitialize(this);
            }
            currentState.LateInitialize(this);
        }

        void Update() {
            var dt = Time.deltaTime;
            currentState.OnTick(this, dt);

            foreach(var p in physics) {
                p.OnTick(this, dt);
            }
        }

        void FixedUpdate()  {
            var dt = Time.fixedDeltaTime * timeFactor;
            currentState.OnFixedTick(this, dt);

            foreach(var p in physics) {
                p.OnFixedTick(this, dt);
            }
        }

#if UNITY_EDITOR
        void OnGUI() {
            if (debugs != null) {
                foreach (var debug in debugs) {
                    debug.OnDrawGUI();
                }
            }
        }

        public void OnDrawGUI() {
            string currentState = $"current state: {this.currentState.ToString()}";

            GUILayout.Label(currentState);
        }
#endif
    }
}

