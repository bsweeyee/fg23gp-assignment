using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lander {
    public interface IEntities {
        public void Initialize(Game game);
        public void OnEnter(Game game, GameState.IBaseGameState previous, GameState.IBaseGameState current);
        public void OnExit(Game game, GameState.IBaseGameState previous, GameState.IBaseGameState current);
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt);
    }
}