using Lander;
using Lander.GameState;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour, IPlayStateEntity, IPlayerObserver
{
    private Transform arrowUI;
    private Image energyUI;
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
        energyUI.fillAmount = Mathf.InverseLerp(0, maxEnergy, currentEnergy);
    }
    
    public void OnVelocityChange(Vector3 currentVelocity) {
    }
    
    void IBaseGameEntity.EarlyInitialize(Game game) {
        if (IsEarlyInitialized) return;

        arrowUI = transform.Find("Arrow");
        arrowUI.gameObject.SetActive(false);

        energyUI = transform.Find("Energy/Bar").GetComponent<Image>();
        jumpArrayUI = new List<GameObject>();
        jumpIcon = transform.Find("Jump/Array/Icon").gameObject;
        jumpIcon.SetActive(false);

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
        jumpIcon.transform.parent.gameObject.SetActive(true);
    }

    void IPlayStateEntity.OnExit(Game game, IBaseGameState previous) {
    }

    void IPlayerObserver.OnEnterState(Player.EPlayerState old, Player.EPlayerState current) {
        switch (current) {
            case Player.EPlayerState.ALIVE:
            energyUI.gameObject.SetActive(true);
            jumpIcon.transform.parent.gameObject.SetActive(true);
            break;
            case Player.EPlayerState.DEAD:
            arrowUI.gameObject.SetActive(false);
            energyUI.gameObject.SetActive(false);
            jumpIcon.transform.parent.gameObject.SetActive(false);
            break;
        }                
    }

    void IPlayerObserver.OnExitState(Player.EPlayerState old, Player.EPlayerState current) {
    }
}
