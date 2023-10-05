using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lander {
    public class Platform : Checkpoint {
        private PlatformGenerator.TileBlockData tileBlockData;

        public PlatformGenerator.TileBlockData TileBlockData {
            get { return tileBlockData; }
            set { tileBlockData = value; }
        }    
    }
}
