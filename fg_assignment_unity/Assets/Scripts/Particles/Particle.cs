using Lander;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    private ParticleSystem ps;
    private ParticleController controller;

    public void Initialize(ParticleController controller) {
        this.controller = controller;
        ps = GetComponent<ParticleSystem>();
        ps.Play();
    }

    public void Tick() {
        if (ps.isStopped) {
            controller.DestroySplashParticle(this);
        }
    }
}
