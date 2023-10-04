using Lander;
using Lander.GameState;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour, IPlayStateEntity, IPlayerObserver
{
    [SerializeField][Range(0, 1)] private float speedWarningOnThreshold = 0.9f; 
    [SerializeField][Range(0, 1)] private float speedWarningOffThreshold = 0.5f; 

    private Transform arrowUI;
    private Transform energyUI;
    private Transform highSpeedUI;
    
    private Image energyBar;
    private Image warningBar;

    private List<GameObject> jumpArrayUI;
    private GameObject jumpIcon;

    public bool IsEarlyInitialized { get; private set; }

    public bool IsLateInitialized { get; private set; }
    
    public void OnBoostDirectionChange(Vector3 boostDirection) {
        if(boostDirection.magnitude == 0) {
            arrowUI.gameObject.SetActive(false);
        }
        else {
            arrowUI.gameObject.SetActive(true);
            arrowUI.right = boostDirection.normalized;
        }
    }
    
    public void OnBoostAmountChange(int currentBoostAmount, int minBoostAmount, int maxBoostAmount) {
        if (jumpIcon == null) return;
        while (jumpArrayUI.Count < maxBoostAmount) {
            var jumpIconInstance = Instantiate(jumpIcon.gameObject, jumpIcon.transform.parent);            
            jumpIconInstance.gameObject.SetActive(true);
            jumpArrayUI.Add(jumpIconInstance);
        }
        for (int i=0; i<jumpArrayUI.Count; i++) {
            if (i < currentBoostAmount) {
                jumpArrayUI[i].SetActive(true);
            }
            else {
                jumpArrayUI[i].SetActive(false);
            }
        }
    }
    
    public void OnEnergyChange(float currentEnergy, float minEnergy, float maxEnergy) {
        energyBar.fillAmount = Mathf.InverseLerp(0, maxEnergy, currentEnergy);
    }
    
    public void OnVelocityChange(Vector3 currentVelocity, float velocityDeathThreshold) {
        if (currentVelocity.magnitude > (velocityDeathThreshold * speedWarningOnThreshold)) {
            highSpeedUI.gameObject.SetActive(true);
        } else if (currentVelocity.magnitude <= (velocityDeathThreshold * speedWarningOffThreshold)) {
            highSpeedUI.gameObject.SetActive(false);
        }

        warningBar.fillAmount = Mathf.InverseLerp(0, velocityDeathThreshold, currentVelocity.magnitude);
    }
    
    void IBaseGameEntity.EarlyInitialize(Game game) {
        if (IsEarlyInitialized) return;

        arrowUI = transform.Find("Arrow");
        arrowUI.gameObject.SetActive(false);

        highSpeedUI = transform.Find("SpeedWarning");
        warningBar = highSpeedUI.Find("Bar").GetComponent<Image>();
        highSpeedUI.gameObject.SetActive(false);

        energyUI = transform.Find("Energy");
        energyBar = energyUI.Find("Bar").GetComponent<Image>();

        jumpArrayUI = new List<GameObject>();
        jumpIcon = transform.Find("Jump/Array/Icon").gameObject;
        if (!jumpIcon.activeInHierarchy) jumpIcon = null;
        jumpIcon?.SetActive(false);

        IsEarlyInitialized = true;
    }
    
    void IBaseGameEntity.LateInitialize(Game game) {
        if (IsLateInitialized) return;

        IsLateInitialized = true;
    }
    
    void IPlayStateEntity.OnTick(Game game, float dt) {
    }
    
    void IPlayStateEntity.OnFixedTick(Game game, float dt) {
    }
    
    void IPlayStateEntity.OnEnter(Game game, IBaseGameState previous) {
        energyUI.gameObject.SetActive(true);
        jumpIcon?.transform.parent.gameObject.SetActive(true);
    }

    void IPlayStateEntity.OnExit(Game game, IBaseGameState previous) {
    }

    void IPlayerObserver.OnEnterState(Player.EPlayerState old, Player.EPlayerState current) {
        switch (current) {
            case Player.EPlayerState.ALIVE:
            energyUI.gameObject.SetActive(true);
            jumpIcon?.transform.parent.gameObject.SetActive(true);
            break;
            case Player.EPlayerState.DEAD:
            arrowUI.gameObject.SetActive(false);
            energyUI.gameObject.SetActive(false);
            highSpeedUI.gameObject.SetActive(false);
            jumpIcon?.transform.parent.gameObject.SetActive(false);
            break;
        }                
    }

    void IPlayerObserver.OnExitState(Player.EPlayerState old, Player.EPlayerState current) {
    }
}
