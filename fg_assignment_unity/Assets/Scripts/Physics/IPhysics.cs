using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lander {
    public interface IPhysics {
        public Vector3 CurrentVelocity { get; }
        public void OnTick(float dt);
        public void OnFixedTick(float dt);
        public void Initialize();
    }
}