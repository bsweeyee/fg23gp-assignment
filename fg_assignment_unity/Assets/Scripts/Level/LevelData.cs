using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lander {
    [System.Serializable]
    public struct LevelData {
        public int LevelID;
        public GameObject StartBlock;
        public GameObject EndBlock;
        public GameObject[] LevelBlocks;
    }
}

