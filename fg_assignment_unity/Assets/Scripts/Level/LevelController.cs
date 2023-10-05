using Lander;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour, IBaseGameEntity {
    public bool IsEarlyInitialized { get; set; }

    public bool IsLateInitialized { get; set; }

    private int currentLevel;

    public void EarlyInitialize(Game game) {
        if (IsEarlyInitialized) return;

        IsEarlyInitialized = true;
    }

    public void LateInitialize(Game game) {
        if (IsLateInitialized) return;

        IsLateInitialized = true;
    }
}
