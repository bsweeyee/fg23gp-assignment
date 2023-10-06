using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lander {
    public class Platform : Checkpoint {
        private PlatformGenerator.PlatformData tileBlockData;

        public PlatformGenerator.PlatformData TileBlockData {
            get { return tileBlockData; }
            set {
                tileBlockData = value;
            }
        }

        public override Vector3 SpawnWorldPosition {
            get {
                if (tileBlockData.LocalSpawnPoint == Vector3.zero) return Vector3.zero;
                return transform.parent.TransformPoint(tileBlockData.LocalSpawnPoint);
            }
        }
    }
}
