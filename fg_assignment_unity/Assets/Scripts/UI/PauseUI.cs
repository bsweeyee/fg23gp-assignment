using Lander;
using Lander.GameState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseUI : MonoBehaviour, IPauseStateEntity {
    public bool IsEarlyInitialized { get; private set; }

    public bool IsLateInitialized { get; private set; }
    
    public void EarlyInitialize(Game game) {
        if (IsEarlyInitialized) return;

        IsEarlyInitialized = true;
    }

    public void LateInitialize(Game game) {
        if (IsLateInitialized) return;

        IsLateInitialized = true;
    }

    public void OnEnter(Game game, IBaseGameState previous) {
        game.PhysicsTickFactor = 0;                     
    }

    public void OnExit(Game game, IBaseGameState current) {
        game.PhysicsTickFactor = 1;
    }

    public void OnFixedTick(Game game, float dt) {
    }

    public void OnTick(Game game, float dt) {
    }
}
