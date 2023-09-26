using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lander {
    public interface IEntities {
        public void Initialize();
        public void Tick(float dt);
        public void FixedTick(float dt);
    }
}