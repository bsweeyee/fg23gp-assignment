using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


namespace Lander {
    namespace GameState {
        public interface IBaseGameState {
            public void EarlyInitialize(Game game);
            public void LateInitialize(Game game);
            public void OnEnter(Game game, IBaseGameState previous, IBaseGameState current);
            public void OnExit(Game game, IBaseGameState previous, IBaseGameState current);
            public void OnTick(Game game, float dt);
            public void OnFixedTick(Game game, float dt);
        }
        
        public abstract class BaseGameState : IBaseGameState {            
            private Game game;

            public virtual void EarlyInitialize(Game game) {                
                foreach(var entity in game.Entities) {                    
                    entity.EarlyInitialize(game);
                }
                this.game = game;                
            }

            public virtual void LateInitialize(Game game) {
                foreach(var entity in game.Entities) {                    
                    entity.LateInitialize(game);
                }
                this.game = game;
            }

            public virtual void OnEnter(Game game, IBaseGameState previous, IBaseGameState current) {
                foreach(var obs in game.Entities) {
                    obs.OnEnter(game, previous, current);
                }
            }            
            public virtual void OnExit(Game game, IBaseGameState previous, IBaseGameState current) {
                foreach(var obs in game.Entities) {
                    obs.OnExit(game, previous, current);
                }
            }            
            public virtual void OnTick(Game game, float dt) {
                foreach(var obs in game.Entities) {
                    obs.OnTick(game, dt);
                }
            }
            public virtual void OnFixedTick(Game game, float dt) {
                foreach(var obs in game.Entities) {
                    obs.OnFixedTick(game, dt);
                }
            }            
        }

        public class StartState : BaseGameState {
            
        }

        public class PlayState : BaseGameState {
            
        }

        public class DeathState : BaseGameState {
           
        }
    }
}