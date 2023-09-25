using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPhysics {
    public Vector3 CurrentVelocity { get; set; }
    public void Evaluate(float dt);
}