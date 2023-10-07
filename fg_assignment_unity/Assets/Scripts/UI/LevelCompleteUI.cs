using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lander.GameState;

namespace Lander {
    public class LevelCompleteUI : MonoBehaviour, ILevelCompleteEntity, ILevelStartEntity {
        public bool IsEarlyInitialized { get; private set; }

        public bool IsLateInitialized { get; private set; }

        [SerializeField] private float sweepSpeed = 2 ;

        private Canvas canvas;
        private Image image;        

        private float normalizedSweepRate;
        private int sweepState;
 
        void IGameInitializeEntity.EarlyInitialize(Game game) {
            if (IsEarlyInitialized) return;

            image = transform.Find("Image").GetComponent<Image>();
            canvas = GetComponent<Canvas>();

            IsEarlyInitialized = true;
        }

        void IGameInitializeEntity.LateInitialize(Game game) {
            if (IsLateInitialized) return;

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = game.CameraController.Camera;

            IsLateInitialized = true;
        }

        void ILevelCompleteEntity.OnEnter(Game game, IBaseGameState previous) {
            gameObject.SetActive(true);
            normalizedSweepRate = 0;
            sweepState = -1;
            image.material.SetFloat("_Progress", 0);
            image.material.SetFloat("_State", sweepState);
        }

        void ILevelCompleteEntity.OnExit(Game game, IBaseGameState current) {
            gameObject.SetActive(false);
        }

        void ILevelCompleteEntity.OnFixedTick(Game game, float dt) {            
        }

        void ILevelCompleteEntity.OnTick(Game game, float dt) {
            normalizedSweepRate += dt * sweepSpeed;
            image.material.SetFloat("_Progress", normalizedSweepRate);
            if (normalizedSweepRate >= 1) {
                game.CurrentState = Game.START_STATE;
            }
        }

        void ILevelStartEntity.OnEnter(Game game, IBaseGameState previous) {
            gameObject.SetActive(true);
            normalizedSweepRate = 0;
            sweepState = 1;
            image.material.SetFloat("_Progress", 0);
            image.material.SetFloat("_State", sweepState);
        }

        void ILevelStartEntity.OnExit(Game game, IBaseGameState current) {
            gameObject.SetActive(false);
        }

        void ILevelStartEntity.OnTick(Game game, float dt) {
            normalizedSweepRate += dt * sweepSpeed;
            image.material.SetFloat("_Progress", normalizedSweepRate);
            if (normalizedSweepRate >= 1) {
                game.CurrentState = Game.PLAY_STATE;
            }
        }

        void ILevelStartEntity.OnFixedTick(Game game, float dt) {
        }
    }
}
