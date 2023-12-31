using Lander;
using Lander.GameState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelEndUI : MonoBehaviour, ILevelEndEntity {
    [SerializeField] private AnimationCurve fade;
    [SerializeField] private float fadeSpeed = 1;
    [SerializeField] private Color fadeColor = Color.white;

    public bool IsEarlyInitialized { get; private set; }

    public bool IsLateInitialized { get; private set; }

    private float normalizedFade;
    private Image fadeImage;
    private Canvas canvas;

    public void EarlyInitialize(Game game) {
        if (IsEarlyInitialized) return;

        fadeImage = transform.Find("Image").GetComponent<Image>();
        canvas = GetComponent<Canvas>();

        gameObject.SetActive(false);            

        IsEarlyInitialized = true;
    }

    public void LateInitialize(Game game) {
        if (IsLateInitialized) return;

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = game.CameraController.Camera;
        
        IsLateInitialized = true;
    }

    void ILevelEndEntity.OnEnter(Game game, IBaseGameState previous) {        
        gameObject.SetActive(true);        
        fadeImage.color = new Color(fadeColor.r,fadeColor.g,fadeColor.b,0);
        normalizedFade = 0;
    }

    void ILevelEndEntity.OnExit(Game game, IBaseGameState current) {
        gameObject.SetActive(false);        
    }

    void ILevelEndEntity.OnFixedTick(Game game, float dt) {
    }

    void ILevelEndEntity.OnTick(Game game, float dt) {
        normalizedFade += dt * fadeSpeed;
        var nf = fade.Evaluate(normalizedFade);
        fadeImage.color = new Color(fadeColor.r,fadeColor.g,fadeColor.b,nf);
        // game.PhysicsTickFactor = Mathf.Clamp01(1 - (nf * 1.1f));        

        if (normalizedFade >= 1) {
            game.CurrentState = Game.LEVEL_TITLE_STATE;
        }
    }
}
