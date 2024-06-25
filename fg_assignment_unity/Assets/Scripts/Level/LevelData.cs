using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Lander {
    [System.Serializable]
    public struct LevelData {
        public int LevelID;
        public GameObject StartBlock;
        public GameObject EndBlock;
        [FormerlySerializedAs("LevelBlocks")] public GameObject[] PlatformBlocks;
    }
}

