using Lander;
using Lander.GameState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseUI : MonoBehaviour, ILevelPauseEntity, IInput {
    public bool IsEarlyInitialized { get; private set; }

    public bool IsLateInitialized { get; private set; }

    private Game game;
    
    public void EarlyInitialize(Game game) {
        if (IsEarlyInitialized) return;
        this.game = game;
        gameObject.SetActive(false);

        IsEarlyInitialized = true;
    }

    public void LateInitialize(Game game) {
        if (IsLateInitialized) return;

        IsLateInitialized = true;
    }

    public void Notify(InputData data) {
        if (data.Paused) {             
            game.CurrentState = Game.PAUSE_STATE;
        }
        else if (!data.Paused && game.CurrentState == Game.PAUSE_STATE) { 
            game.CurrentState = Game.PLAY_STATE;
        }
    }

    public void OnEnter(Game game, IBaseGameState previous) {
        game.PhysicsTickFactor = 0;
        game.CurrentStateTickFactor = 0;
        gameObject.SetActive(true);                     
    }

    public void OnExit(Game game, IBaseGameState current) {
        game.PhysicsTickFactor = 1;
        game.CurrentStateTickFactor = 1;
        gameObject.SetActive(false);
    }

    public void OnFixedTick(Game game, float dt) {
    }

    public void OnTick(Game game, float dt) {
    }
}
