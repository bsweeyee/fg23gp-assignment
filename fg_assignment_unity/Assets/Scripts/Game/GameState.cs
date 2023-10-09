using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


namespace Lander {
    namespace GameState {
        public interface IBaseGameState {
            public void OnEnter(Game game, IBaseGameState previous);
            public void OnExit(Game game, IBaseGameState current);
            public void OnTick(Game game, float dt);
            public void OnFixedTick(Game game, float dt);
        }
        
        public abstract class BaseGameState : IBaseGameState {            
            private Game game;
            protected IGameInitializeEntity[] entities;
            public IGameInitializeEntity[] Entities {
                get { return entities; }
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
            public override void OnEnter(Game game, IBaseGameState previous) {
                entities = GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<ILevelStartEntity>().ToArray();
                foreach(var obs in entities) {
                    var o = obs as ILevelStartEntity;
                    o.OnEnter(game, previous);
                }
            }            
            public override void OnExit(Game game, IBaseGameState current) {
                foreach(var obs in entities) {
                    var o = obs as ILevelStartEntity;
                    o.OnExit(game, current);
                }
            }            
            public override void OnTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as ILevelStartEntity;                    
                    o.OnTick(game, dt);
                }
            }
            public override void OnFixedTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as ILevelStartEntity;
                    o.OnFixedTick(game, dt);
                }
            }  
        }

        public class PlayState : BaseGameState {
            public override void OnEnter(Game game, IBaseGameState previous) {
                entities = GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<ILevelPlayEntity>().ToArray();
                foreach(var obs in entities) {
                    var o = obs as ILevelPlayEntity;                    
                    o.OnEnter(game, previous);
                }
            }            
            public override void OnExit(Game game, IBaseGameState current) {
                foreach(var obs in entities) {
                    var o = obs as ILevelPlayEntity;
                    o.OnExit(game, current);
                }
            }            
            public override void OnTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as ILevelPlayEntity;
                    o.OnTick(game, dt);
                }
            }
            public override void OnFixedTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as ILevelPlayEntity;
                    o.OnFixedTick(game, dt);
                }
            }  
        }        

        public class PauseState : BaseGameState {
            public override void OnEnter(Game game, IBaseGameState previous) {
                entities = GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<ILevelPauseEntity>().ToArray();
                foreach(var obs in entities) {
                    var o = obs as ILevelPauseEntity;
                    o.OnEnter(game, previous);
                }
            }            
            public override void OnExit(Game game, IBaseGameState current) {
                foreach(var obs in entities) {
                    var o = obs as ILevelPauseEntity;
                    o.OnExit(game, current);
                }
            }            
            public override void OnTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as ILevelPauseEntity;
                    o.OnTick(game, dt);
                }
            }
            public override void OnFixedTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as ILevelPauseEntity;
                    o.OnFixedTick(game, dt);
                }
            }
        }
        
        public class LevelCompleteState : BaseGameState {
            public override void OnEnter(Game game, IBaseGameState previous) {
                entities = GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<ILevelCompleteEntity>().ToArray();
                foreach(var obs in entities) {
                    var o = obs as ILevelCompleteEntity;
                    o.OnEnter(game, previous);
                }
            }            
            public override void OnExit(Game game, IBaseGameState current) {
                foreach(var obs in entities) {
                    var o = obs as ILevelCompleteEntity;
                    o.OnExit(game, current);
                }
            }            
            public override void OnTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as ILevelCompleteEntity;
                    o.OnTick(game, dt);
                }
            }
            public override void OnFixedTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as ILevelCompleteEntity;
                    o.OnFixedTick(game, dt);
                }
            }
        }

        public class LevelEndState : BaseGameState {
            public override void OnEnter(Game game, IBaseGameState previous) {
                entities = GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<ILevelEndEntity>().ToArray();
                foreach(var obs in entities) {
                    var o = obs as ILevelEndEntity;
                    o.OnEnter(game, previous);
                }
            }            
            public override void OnExit(Game game, IBaseGameState current) {
                foreach(var obs in entities) {
                    var o = obs as ILevelEndEntity;
                    o.OnExit(game, current);
                }
            }            
            public override void OnTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as ILevelEndEntity;
                    o.OnTick(game, dt);
                }
            }
            public override void OnFixedTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as ILevelEndEntity;
                    o.OnFixedTick(game, dt);
                }
            }
        }

        public class LevelTitleState : BaseGameState {
            public override void OnEnter(Game game, IBaseGameState previous) {
                entities = GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<ILevelTitleEntity>().ToArray();
                foreach(var obs in entities) {
                    var o = obs as ILevelTitleEntity;
                    o.OnEnter(game, previous);
                }
            }            
            public override void OnExit(Game game, IBaseGameState current) {
                foreach(var obs in entities) {
                    var o = obs as ILevelTitleEntity;
                    o.OnExit(game, current);
                }
            }            
            public override void OnTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as ILevelTitleEntity;
                    o.OnTick(game, dt);
                }
            }
            public override void OnFixedTick(Game game, float dt) {
                foreach(var obs in entities) {
                    var o = obs as ILevelTitleEntity;
                    o.OnFixedTick(game, dt);
                }
            }
        }
    }
}