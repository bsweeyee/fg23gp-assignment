using Lander;
using Lander.GameState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour, IGameStateEntity, IPlayerObserver
{
    private Transform arrowUI;
    private Image energyUI;
    public void EarlyInitialize(Game game) {
        arrowUI = transform.Find("Arrow");
        arrowUI.gameObject.SetActive(false);

        energyUI = transform.Find("Energy/Bar").GetComponent<Image>();
    }
    public void LateInitialize(Game game) {
    }
    public void OnTick(Game game, float dt) {
    }
    public void OnFixedTick(Game game, float dt) {
    }
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
    }
    public void OnEnergyChange(float currentEnergy, float minEnergy, float maxEnergy) {
        energyUI.fillAmount = Mathf.InverseLerp(0, maxEnergy, currentEnergy);
    }
    public void OnVelocityChange(Vector3 currentVelocity) {
    }
    public void OnEnter(Game game, IBaseGameState previous, IBaseGameState current) {
    }
    public void OnExit(Game game, IBaseGameState previous, IBaseGameState current) {
    }
}
