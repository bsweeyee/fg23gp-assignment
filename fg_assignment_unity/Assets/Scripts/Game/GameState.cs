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
            public void OnEnter(Game game, IBaseGameState previous);
            public void OnExit(Game game, IBaseGameState current);
            public void OnTick(Game game, float dt);
            public void OnFixedTick(Game game, float dt);
        }
        
        public abstract class BaseGameState : IBaseGameState {            
            private Game game;
            protected IBaseGameEntity[] entities;
            public IBaseGameEntity[] Entities {
                get { return entities; }
            }

            public virtual void EarlyInitialize(Game game) { 
                foreach(var entity in entities) {                    
                    entity.EarlyInitialize(game);
                }
                this.game = game;                
            }

            public virtual void LateInitialize(Game game) {
                foreach(var entity in entities) {                    
                    entity.LateInitialize(game);
                }
                this.game = game;
            }

            public virtual void OnEnter(Game game, IBaseGameState previous) {
            }

            public virtual void OnExit(Game game, IBaseGameState current) {
            }

            public virtual void OnTick(Game game, float dt) {
            }

            public virtual void OnFixedTick(Game game, float dt) {
            }
        }

        public class StartState : BaseGameState {
            public override void EarlyInitialize(Game game) {
                entities = GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<IStartStateEntity>().ToArray();
                base.EarlyInitialize(game);
            }

            public override void OnEnter(Game game, IBaseGameState previous) {
                foreach(var obs in entities) {
                    var o = obs as IStartStateEntity;
                    o.OnEnter(game, previous);
                }
            }            
            public override void OnExit(Game game, IBaseGameState current) {
                foreach(var obs in entities) {
                    var o = obs as IStartStateEntity;
                    o.OnExit(game, current);
                }
            }            
            public override void OnTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as IStartStateEntity;
                    o.OnTick(game, dt);
                }
            }
            public override void OnFixedTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as IStartStateEntity;
                    o.OnFixedTick(game, dt);
                }
            }  
        }

        public class PlayState : BaseGameState {
            public override void EarlyInitialize(Game game) {
                entities = GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<IPlayStateEntity>().ToArray();
                base.EarlyInitialize(game);
            }
            public override void OnEnter(Game game, IBaseGameState previous) {
                foreach(var obs in entities) {
                    var o = obs as IPlayStateEntity;
                    o.OnEnter(game, previous);
                }
            }            
            public override void OnExit(Game game, IBaseGameState current) {
                foreach(var obs in entities) {
                    var o = obs as IPlayStateEntity;
                    o.OnExit(game, current);
                }
            }            
            public override void OnTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as IPlayStateEntity;
                    o.OnTick(game, dt);
                }
            }
            public override void OnFixedTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as IPlayStateEntity;
                    o.OnFixedTick(game, dt);
                }
            }  
        }        

        public class PauseState : BaseGameState {
            public override void EarlyInitialize(Game game) {
                entities = GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<IPauseStateEntity>().ToArray();
                base.EarlyInitialize(game);
            }

            public override void OnEnter(Game game, IBaseGameState previous) {
                foreach(var obs in entities) {
                    var o = obs as IPauseStateEntity;
                    o.OnEnter(game, previous);
                }
            }            
            public override void OnExit(Game game, IBaseGameState current) {
                foreach(var obs in entities) {
                    var o = obs as IPauseStateEntity;
                    o.OnExit(game, current);
                }
            }            
            public override void OnTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as IPauseStateEntity;
                    o.OnTick(game, dt);
                }
            }
            public override void OnFixedTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as IPauseStateEntity;
                    o.OnFixedTick(game, dt);
                }
            }
        }
    }
}