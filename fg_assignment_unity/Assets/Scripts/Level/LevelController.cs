using Lander;
using Lander.GameState;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Lander {
    public class LevelController : MonoBehaviour, ILevelStartEntity, ILevelCompleteEntity, ILevelEndEntity {
        public bool IsEarlyInitialized { get; set; }
        public bool IsLateInitialized { get; set; }

        bool IGameInitializeEntity.IsEarlyInitialized => throw new System.NotImplementedException();

        bool IGameInitializeEntity.IsLateInitialized => throw new System.NotImplementedException();

        private LevelData[] data;
        private LevelData current;

        private List<GameObject> levelInstances; 
        private int currentLevel = 0;               

        public void EarlyInitialize(Game game) {
            if (IsEarlyInitialized) return;

            
            levelInstances = new List<GameObject>();

            IsEarlyInitialized = true;
        }

        public void LateInitialize(Game game) {
            if (IsLateInitialized) return;

            IsLateInitialized = true;
        }

        public void DestroyLevels() {
            foreach(var instance in levelInstances) {
                Destroy(instance);
            }
            levelInstances.Clear();
        }

        public void GenerateLevels(LevelData data, float length, float width) {
            var start = data.StartBlock;
            var end = data.EndBlock;
            var levels = data.LevelBlocks;

            var startInstance = Instantiate(start);
            levelInstances.Add(startInstance);
            for(int i = 0; i < levels.Length; i++) {
                var levelInstance = Instantiate(levels[i]);
                levelInstance.transform.position = new Vector3(0, length * i, 0);
                levelInstances.Add(levelInstance);
            }
            var endInstance = Instantiate(end);
            endInstance.transform.position = new Vector3(0, data.LevelBlocks.Length * length, 0);
            levelInstances.Add(endInstance);

            var platformGenerator = startInstance.GetComponentsInChildren<PlatformGenerator>().First(x => x.AxisDirection == PlatformGenerator.EAxisDirection.X);
            if(platformGenerator.LocalSpaceBlocks != null && platformGenerator.LocalSpaceBlocks.Count > 0) {
                Checkpoint.CurrentSpawnWorldPosition = platformGenerator.LocalSpaceBlocks[0].LocalSpawnPoint;
            }           
        }

        void ILevelStartEntity.OnEnter(Game game, IBaseGameState previous) {
            data = game.GameSettings.LevelData;
            current = data.First(x=> x.LevelID == currentLevel);
            
            DestroyLevels();
            GenerateLevels(current, game.GameSettings.LevelLength, game.GameSettings.LevelWidth);
             foreach(var instance in levelInstances) {
                var pgs = instance.GetComponentsInChildren<PlatformGenerator>();
                foreach(var pg in pgs) {
                    pg.EarlyInitialize(game);
                    pg.GeneratePlatform(game.GameSettings);
                }
            }

            Checkpoint.Respawn(game.Player.transform);
            game.CameraController.FollowTarget = game.Player.transform;
            game.CameraController.transform.position = game.CameraController.TargetPosition;
        }

        void ILevelStartEntity.OnExit(Game game, IBaseGameState current) {
        }

        void ILevelStartEntity.OnTick(Game game, float dt) {
        }

        void ILevelStartEntity.OnFixedTick(Game game, float dt) {
        }

        void ILevelCompleteEntity.OnEnter(Game game, IBaseGameState previous) {
            currentLevel += 1;
            if (currentLevel >= game.GameSettings.LevelData.Length) {
                game.CurrentState = Game.LEVEL_END_STATE;
            }
        }

        void ILevelCompleteEntity.OnExit(Game game, IBaseGameState current) {
        }

        void ILevelCompleteEntity.OnTick(Game game, float dt) {
        }

        void ILevelCompleteEntity.OnFixedTick(Game game, float dt) {
        }

        void ILevelEndEntity.OnEnter(Game game, IBaseGameState previous) {
        }

        void ILevelEndEntity.OnExit(Game game, IBaseGameState current) {
        }

        void ILevelEndEntity.OnTick(Game game, float dt) {
        }

        void ILevelEndEntity.OnFixedTick(Game game, float dt) {
        }        
    }
}
