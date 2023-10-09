using Lander;
using Lander.GameState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour, ILevelTitleEntity, ILevelCompleteEntity {
    public bool IsEarlyInitialized { get; private set; }

    public bool IsLateInitialized { get; private set; }
    
    [SerializeField] private float fadeTransitionSpeed = 0.1f;
    [SerializeField] private AnimationCurve fadeTransitionCurve;

    private Canvas canvas;

    private Image fade;
    private Button playAgainButton;

    private float normalizedFadeTransitionRate;

    public void EarlyInitialize(Game game) {
        if (IsEarlyInitialized) return;

        gameObject.SetActive(false);
        canvas = GetComponent<Canvas>();

        fade = transform.Find("Fade").GetComponent<Image>();
        playAgainButton = transform.Find("PlayAgain").GetComponent<Button>(); 

        fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, 0);            

        playAgainButton.gameObject.SetActive(false);

        playAgainButton.onClick.AddListener( () => {
            game.LevelController.CurrentLevel = -1;
            game.CurrentState = Game.LEVEL_COMPLETE_STATE;
        });

        IsEarlyInitialized = true;
    }

    public void LateInitialize(Game game) {
        if (IsLateInitialized) return;

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = game.CameraController.Camera;

        IsLateInitialized = true;
    }

    void ILevelTitleEntity.OnEnter(Game game, IBaseGameState previous) {
        gameObject.SetActive(true);
        fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, 1);            
        normalizedFadeTransitionRate = 0;
        playAgainButton.gameObject.SetActive(false);
    }

    void ILevelTitleEntity.OnExit(Game game, IBaseGameState current) {
    }

    void ILevelTitleEntity.OnFixedTick(Game game, float dt) {
    }

    void ILevelTitleEntity.OnTick(Game game, float dt) {
        if (normalizedFadeTransitionRate < 1) {
            normalizedFadeTransitionRate += dt * fadeTransitionSpeed;
            fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, fadeTransitionCurve.Evaluate(normalizedFadeTransitionRate));            
        }
        else {
            playAgainButton.gameObject.SetActive(true);                                                
        }    
    }

    void ILevelCompleteEntity.OnEnter(Game game, IBaseGameState previous) {
    }

    void ILevelCompleteEntity.OnExit(Game game, IBaseGameState current) {
        gameObject.SetActive(false);
    }

    void ILevelCompleteEntity.OnTick(Game game, float dt) {
    }

    void ILevelCompleteEntity.OnFixedTick(Game game, float dt) {
    }
}
