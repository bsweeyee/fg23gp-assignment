using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lander {
    public interface IBaseEntity {
        public void EarlyInitialize(Game game);
        public void LateInitialize(Game game);
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt);        
    }

    public interface IGameStateEntity : IBaseEntity {
        public void OnEnter(Game game, GameState.IBaseGameState previous, GameState.IBaseGameState current);
        public void OnExit(Game game, GameState.IBaseGameState previous, GameState.IBaseGameState current);
    }
}