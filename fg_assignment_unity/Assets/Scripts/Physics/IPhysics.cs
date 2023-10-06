using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lander {
    namespace Physics {
        public interface IPhysics : IGameInitializeEntity, IGameTickEntity {
            public Vector3 CurrentVelocity { get; }
        }
    }
}
