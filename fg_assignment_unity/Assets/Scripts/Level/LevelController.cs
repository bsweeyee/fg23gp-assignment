using Lander;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lander {
    public class LevelController : MonoBehaviour, IBaseGameEntity {
        public bool IsEarlyInitialized { get; set; }
        public bool IsLateInitialized { get; set; }

        private LevelData[] data;
        private Platform start;
        private Platform end;

        public static int CurrentLevel = 0;

        public void EarlyInitialize(Game game) {
            if (IsEarlyInitialized) return;

            data = game.GameSettings.LevelData;
            GenerateLevel(data.First(x=> x.LevelID == CurrentLevel));

            IsEarlyInitialized = true;
        }

        public void LateInitialize(Game game) {
            if (IsLateInitialized) return;

            IsLateInitialized = true;
        }

        public void GenerateLevel(LevelData data) {
            var start = data.StartBlock;
            var end = data.EndBlock;
            var levels = data.LevelBlocks;

            var startInstance = Instantiate(start);
            for(int i = 0; i < levels.Length; i++) {
                var levelInstance = Instantiate(levels[i]);
                levelInstance.transform.position = new Vector3(0, data.LevelLength * i, 0);
            }
            var endInstance = Instantiate(end);
            endInstance.transform.position = new Vector3(0, data.LevelBlocks.Length * data.LevelLength, 0);

            var platformGenerator = startInstance.GetComponentsInChildren<PlatformGenerator>().First(x => x.AxisDirection == PlatformGenerator.EAxisDirection.X);
            if(platformGenerator.LocalSpaceBlocks != null && platformGenerator.LocalSpaceBlocks.Count > 0) {
                Checkpoint.CurrentSpawnWorldPosition = platformGenerator.LocalSpaceBlocks[0].LocalSpawnPoint;
            }
        }
    }
}
