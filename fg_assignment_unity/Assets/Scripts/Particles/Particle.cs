using Lander;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    protected ParticleSystem ps;
    protected ParticleController controller;

    public virtual void Initialize(ParticleController controller) {
        this.controller = controller;
        ps = GetComponent<ParticleSystem>();
        ps.Play();
    }

    public virtual void Tick() {
        
    }

    public void Play() {
        ps.Play();
    }

    public void Stop() {
        ps.Stop();
    }

    public void Pause() {
        ps.Pause();
    }
}
