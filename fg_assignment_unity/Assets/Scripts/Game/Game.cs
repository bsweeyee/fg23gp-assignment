using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Linq;
using UnityEditor;
using Lander.GameState;
using System;
using Unity.VisualScripting;

namespace Lander {
    public class Game : MonoBehaviour, IDebug
    {
        public static PlayState PLAY_STATE;
        public static StartState START_STATE;
        public static GameState.PauseState PAUSE_STATE;

        public static Game instance;

        [SerializeField][Range(0, 1)] private float physicsTickFactor = 1;
        [SerializeField][Range(0, 1)] private float currentStateTickFactor = 1;


        private InputController inputController;
        private Physics.IPhysics[] physics;
        private IDebug[] debugs;
        private BaseGameState currentState;
        private GameSettings gameSettings;

        public BaseGameState CurrentState {
            get { return currentState; }
            set {
                var previousState = currentState;
                currentState = value;

                if (previousState != null) {
                    previousState.OnExit(this, currentState);
                }

                if (currentState != null) {
                    currentState.OnEnter(this, previousState);
                }
            }
        }

        public float PhysicsTickFactor {
            get { return physicsTickFactor; }
            set { physicsTickFactor = value; }
        }

        public float CurrentStateTickFactor {
            get { return currentStateTickFactor; }
            set { currentStateTickFactor = value; }
        }

        public GameSettings GameSettings {
            get {
                return gameSettings;
            }
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
            inputController = FindObjectOfType<InputController>(true);
            physics = FindObjectsOfType<MonoBehaviour>(true).OfType<Physics.IPhysics>().ToArray();            
            debugs = GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<IDebug>().ToArray();
            var baseEntities = FindObjectsOfType<MonoBehaviour>(true).OfType<IBaseGameEntity>().ToArray();

            START_STATE = new StartState();
            PLAY_STATE = new PlayState();
            PAUSE_STATE = new GameState.PauseState();

            START_STATE.EarlyInitialize(this);
            PLAY_STATE.EarlyInitialize(this);
            PAUSE_STATE.EarlyInitialize(this);

            // we have to run initialize here for all other entities that are not part of the state
            foreach(var e in baseEntities) {
                e.EarlyInitialize(this);
            }
        }

        void LateInitialize() {
            START_STATE.LateInitialize(this);
            PLAY_STATE.LateInitialize(this);
            PAUSE_STATE.LateInitialize(this);
            
            var baseEntities = FindObjectsOfType<MonoBehaviour>(true).OfType<IBaseGameEntity>().ToArray();
            
            // we have to run initialize here for all other entities that are not part of the state
            foreach(var e in baseEntities) {
                e.LateInitialize(this);
            }  
        }

        void Update() {
            var dt = Time.deltaTime * currentStateTickFactor;
            currentState.OnTick(this, dt * currentStateTickFactor);

            foreach(var p in physics) {
                p.OnTick(this, dt * physicsTickFactor);
            }
        }

        void FixedUpdate()  {
            var dt = Time.fixedDeltaTime;
            currentState.OnFixedTick(this, dt * currentStateTickFactor);

            foreach(var p in physics) {
                p.OnFixedTick(this, dt * physicsTickFactor);
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

