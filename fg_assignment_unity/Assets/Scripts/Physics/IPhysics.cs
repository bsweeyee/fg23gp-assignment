using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lander {
    public interface IPhysics : IBaseEntity {
        public Vector3 CurrentVelocity { get; }       
    }
}