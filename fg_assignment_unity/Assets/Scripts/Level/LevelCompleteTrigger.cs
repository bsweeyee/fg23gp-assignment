using Lander.GameState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Lander {
    public class LevelCompleteTrigger : BoxTrigger, ILevelPlayEntity
    {        
        public bool IsEarlyInitialized { get; set; }

        public bool IsLateInitialized { get; set; }

        public virtual void EarlyInitialize(Game game) {
            if (IsEarlyInitialized) return;
            

            IsEarlyInitialized = true;
        }

        public virtual void LateInitialize(Game game) {
            if (IsLateInitialized) return;

            IsLateInitialized = true;
        }       

        public void OnEnter(Game game, IBaseGameState previous) {                        
            Initialize();
            onEnterTrigger.AddListener( (Collider2D collider, float dt) => {
                game.LevelController.CurrentLevel += 1;
                if (game.LevelController.CurrentLevel >= game.GameSettings.LevelData.Length) {
                    game.CurrentState = Game.LEVEL_END_STATE;
                }
                else {
                    game.CurrentState = Game.LEVEL_COMPLETE_STATE;
                }
            });            
        }

        public void OnExit(Game game, IBaseGameState current) {
        }

        public void OnFixedTick(Game game, float dt) {
        }

        public void OnTick(Game game, float dt) {
            OnTriggerCheck(game.GameSettings.TriggerLayer, dt);
        }
    }
}
