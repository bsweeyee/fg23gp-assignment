using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Linq;
using UnityEditor;
using Lander.GameState;
using System;
using Unity.VisualScripting;
using System.Drawing;

namespace Lander {
    public class Game : MonoBehaviour, IDebug
    {
        public static PlayState PLAY_STATE;
        public static StartState START_STATE;
        public static GameState.PauseState PAUSE_STATE;
        public static LevelCompleteState LEVEL_COMPLETE_STATE;
        public static LevelEndState LEVEL_END_STATE;
        public static LevelTitleState LEVEL_TITLE_STATE;

        public static Game instance;

        [SerializeField][Range(0, 1)] private float physicsTickFactor = 1;
        [SerializeField][Range(0, 1)] private float currentStateTickFactor = 1;


        private InputController inputController;
        private LevelController levelController;
        private CameraController cameraController;
        private InteractorController physicsInteractorController;
        private ParticleController particleController;
        private Player player;
        private Physics.IPhysics[] physics;
        private IDebug[] debugs;
        private BaseGameState currentState;
        private GameSettings gameSettings;

        public Player Player {
            get {
                return player;
            }
        }

        public CameraController CameraController {
            get {
                return cameraController;
            }
        }

        public ParticleController ParticleController {
            get {
                return particleController;
            }
        }

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

        public LevelController LevelController {
            get {
                return levelController;
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

        public void Initialize(GameSettings settings) {
            if (instance == null) {
                instance = this;
                DontDestroyOnLoad(gameObject);

                gameSettings = settings;

                EarlyInitialize();
                LateInitialize();

                return;
            }

            Destroy(gameObject);
        }
       
        void EarlyInitialize() {
            inputController = FindObjectOfType<InputController>(true);
            levelController = FindObjectOfType<LevelController>(true);
            cameraController = FindObjectOfType<CameraController>(true);
            physicsInteractorController = FindObjectOfType<InteractorController>(true);
            particleController = FindObjectOfType<ParticleController>(true);
            player = FindObjectOfType<Player>(true);

            inputController?.EarlyInitialize(this);
            levelController?.EarlyInitialize(this);
            cameraController?.EarlyInitialize(this);
            physicsInteractorController?.EarlyInitialize(this);
            particleController?.EarlyInitialize(this);
            player?.EarlyInitialize(this);

            physics = FindObjectsOfType<MonoBehaviour>(true).OfType<Physics.IPhysics>().ToArray();
            debugs = GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<IDebug>().ToArray();
            var baseEntities = FindObjectsOfType<MonoBehaviour>(true).OfType<IGameInitializeEntity>().ToArray();
            
            START_STATE = new StartState();
            PLAY_STATE = new PlayState();
            PAUSE_STATE = new GameState.PauseState();
            LEVEL_COMPLETE_STATE = new LevelCompleteState();
            LEVEL_END_STATE = new LevelEndState(); 
            LEVEL_TITLE_STATE = new LevelTitleState();           

            // we have to run initialize here for all other entities that are not part of the state
            foreach(var e in baseEntities) {
                e.EarlyInitialize(this);
            }
        }

        void LateInitialize() {            
            var baseEntities = FindObjectsOfType<MonoBehaviour>(true).OfType<IGameInitializeEntity>().ToArray();

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

        void OnGUI() {            
            if (debugs != null) {
                foreach (var debug in debugs) {
                    debug.OnDrawGUI();
                }
            }
        }        

    public void OnDrawGUI() {
#if DISPLAY_DEBUG_MENU
            string currentState = $"current state: {this.currentState.ToString()}";

            GUILayout.Label(currentState);
            using (var horizontalScope = new GUILayout.HorizontalScope()) {
                if (GUILayout.Button("PLAY")) {
                    var pgs = FindObjectsOfType<PlatformGenerator>();
                    foreach(var pg in pgs) {
                        pg.EarlyInitialize(this);
                        pg.GeneratePlatform(GameSettings);
                    }
                    if (pgs.Length > 0) {
                        var platformGenerator = pgs.First(x => x.AxisDirection == PlatformGenerator.EAxisDirection.X);
                        if(platformGenerator.LocalSpaceBlocks != null && platformGenerator.LocalSpaceBlocks.Count > 0) {
                            Checkpoint.CurrentSpawnWorldPosition = platformGenerator.LocalSpaceBlocks[0].LocalSpawnPoint;
                        }
                    }
                    Checkpoint.Respawn(player.transform);
                    cameraController.FollowTarget = player.transform;
                    cameraController.transform.position = cameraController.TargetPosition;
                    CurrentState = PLAY_STATE;                    
                }
                if(GUILayout.Button("LEVEL COMPLETE")) {
                    CurrentState = LEVEL_COMPLETE_STATE;
                }
                if(GUILayout.Button("LEVEL END")) {
                    CurrentState = LEVEL_END_STATE;
                }
            }
#endif
        }
    }
}

