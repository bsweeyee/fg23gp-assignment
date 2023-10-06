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
            onTrigger = new UnityEvent();
            onTrigger.AddListener( () => {
                game.CurrentState = Game.LEVEL_COMPLETE_STATE;
            });            
        }

        public void OnExit(Game game, IBaseGameState current) {
        }

        public void OnFixedTick(Game game, float dt) {
        }

        public void OnTick(Game game, float dt) {
            OnTriggerCheck(game.GameSettings.TriggerLayer);
        }
    }
}
