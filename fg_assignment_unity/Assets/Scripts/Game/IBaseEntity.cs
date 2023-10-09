using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lander {
    public interface IGameInitializeEntity {
        public bool IsEarlyInitialized { get; }
        public bool IsLateInitialized { get; } 
        public void EarlyInitialize(Game game);
        public void LateInitialize(Game game);            
    }

    public interface IGameTickEntity {
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt);    
    }    

    public interface ILevelPlayEntity : IGameInitializeEntity {
        public void OnEnter(Game game, GameState.IBaseGameState previous);
        public void OnExit(Game game, GameState.IBaseGameState current);
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt);        
    }

    public interface ILevelStartEntity : IGameInitializeEntity {
        public void OnEnter(Game game, GameState.IBaseGameState previous);
        public void OnExit(Game game, GameState.IBaseGameState current);
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt);  
    }

    public interface ILevelPauseEntity : IGameInitializeEntity {
        public void OnEnter(Game game, GameState.IBaseGameState previous);
        public void OnExit(Game game, GameState.IBaseGameState current);
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt);  
    }

    public interface ILevelCompleteEntity : IGameInitializeEntity {
        public void OnEnter(Game game, GameState.IBaseGameState previous);
        public void OnExit(Game game, GameState.IBaseGameState current);
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt);  
    }

    public interface ILevelEndEntity : IGameInitializeEntity {
        public void OnEnter(Game game, GameState.IBaseGameState previous);
        public void OnExit(Game game, GameState.IBaseGameState current);
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt);  
    }

    public interface ILevelTitleEntity : IGameInitializeEntity {
        public void OnEnter(Game game, GameState.IBaseGameState previous);
        public void OnExit(Game game, GameState.IBaseGameState current);
        public void OnTick(Game game, float dt);
        public void OnFixedTick(Game game, float dt); 
    }
}