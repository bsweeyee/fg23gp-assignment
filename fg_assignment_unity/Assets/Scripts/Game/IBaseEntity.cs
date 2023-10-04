using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lander {
    public interface IBaseGameEntity {
        public bool IsEarlyInitialized { get; }
        public bool IsLateInitialized { get; } 
        public void EarlyInitialize(Game game);
        public void LateInitialize(Game game);            
    }

    public interface IBaseGameTickEntity {
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt);    
    }    

    public interface IPlayStateEntity : IBaseGameEntity {
        public void OnEnter(Game game, GameState.IBaseGameState previous);
        public void OnExit(Game game, GameState.IBaseGameState current);
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt);        
    }

    public interface IStartStateEntity : IBaseGameEntity {
        public void OnEnter(Game game, GameState.IBaseGameState previous);
        public void OnExit(Game game, GameState.IBaseGameState current);
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt);  
    }

    public interface IPauseStateEntity : IBaseGameEntity {
        public void OnEnter(Game game, GameState.IBaseGameState previous);
        public void OnExit(Game game, GameState.IBaseGameState current);
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt);  
    }
}