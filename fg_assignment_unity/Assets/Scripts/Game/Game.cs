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
                
        private InputController inputController;
        private IEntities[] entities;
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

        public IEntities[] Entities {
            get { return entities; }
        }

        public void Initialize() {
            if (instance == null) {
                instance = this;
                DontDestroyOnLoad(gameObject);
                
                InitInput();                
                InitGameState();
                InitPhysics();
                InitDebug();

                return;
            }

            Destroy(gameObject);
        }

        void InitInput() {
            inputController = FindObjectOfType<InputController>();
            inputController.Initialize();
        }        

        void InitPhysics() {
            physics = FindObjectsOfType<MonoBehaviour>().OfType<IPhysics>().ToArray();
            foreach(var p in physics) {
                p.Initialize();
            }
        }

        void InitGameState() {
            entities = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IEntities>().ToArray();

            START_STATE = new StartState();
            PLAY_STATE = new PlayState();
            DEATH_STATE = new DeathState();            

            currentState = PLAY_STATE;
            
            START_STATE.Initialize(this);
            PLAY_STATE.Initialize(this);
            DEATH_STATE.Initialize(this);
        }
        
        void InitDebug() {
            debugs = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IDebug>().ToArray();            
        }

        void Update() {
            var dt = Time.deltaTime;           
            currentState.OnTick(this, dt);

            foreach(var p in physics) {
                p.OnTick(dt);
            }
        }

        void FixedUpdate()  {
            var dt = Time.fixedDeltaTime;
            currentState.OnFixedTick(this, dt);

            foreach(var p in physics) {
                p.OnFixedTick(dt);
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

