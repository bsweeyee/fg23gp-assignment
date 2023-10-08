using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lander {
    public class SplashParticle : Particle
    {
        public override void Tick() {
            if (ps.isStopped) {
                controller.DestroyParticle(this);
            }
        }
    }
}
