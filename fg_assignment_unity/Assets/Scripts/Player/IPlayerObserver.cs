using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  Lander {
    public interface IPlayerObserver {
        public void OnBoostDirectionChange(Vector3 boostDirection);
        public void OnBoostAmountChange(int currentBoostAmount, int minBoostAmount, int maxBoostAmount);
        public void OnEnergyChange(float currentEnergy, float minEnergy, float maxEnergy);
        public void OnVelocityChange(Vector3 currentVelocity);
    }
}
