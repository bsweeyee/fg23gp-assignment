using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lander {
    public interface IBaseEntity {
        public bool IsEarlyInitialized { get; }
        public bool IsLateInitialized { get; } 
        public void EarlyInitialize(Game game);
        public void LateInitialize(Game game);            
    }

    public interface IBaseTickEntity {
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt);    
    }    

    public interface IPlayStateEntity : IBaseEntity {
        public void OnEnter(Game game, GameState.IBaseGameState previous);
        public void OnExit(Game game, GameState.IBaseGameState current);
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt);        
    }

    public interface IStartStateEntity : IBaseEntity {
        public void OnEnter(Game game, GameState.IBaseGameState previous);
        public void OnExit(Game game, GameState.IBaseGameState current);
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt);  
    }

    public interface IDeathStateEntity : IBaseEntity {
        public void OnEnter(Game game, GameState.IBaseGameState previous);
        public void OnExit(Game game, GameState.IBaseGameState current);
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt);  
    }

    public interface IPauseStateEntity : IBaseEntity {
        public void OnEnter(Game game, GameState.IBaseGameState previous);
        public void OnExit(Game game, GameState.IBaseGameState current);
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt);  
    }
}