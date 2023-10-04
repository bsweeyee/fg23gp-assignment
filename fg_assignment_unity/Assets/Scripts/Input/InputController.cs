using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Data.Common;

namespace Lander {
    [System.Serializable]
    public struct InputData {
        public enum EBoostState {
            NONE,
            PRESSED,            
            RELEASED
        }

        public Vector2 Movement;
        public EBoostState BoostState;        
    }

    public interface IInput {
        public void Notify(InputData data);
    }

    public class InputController : MonoBehaviour, IBaseEntity
    {
        private IInput[] inputs;
        private InputData cachedInput;

        public bool IsEarlyInitialized { get; private set; }

        public bool IsLateInitialized { get; private set; }

        public void EarlyInitialize(Game game) {
            if (IsEarlyInitialized) return;

            inputs = FindObjectsOfType<MonoBehaviour>().OfType<IInput>().ToArray();
            cachedInput = new InputData();

            cachedInput.Movement = Vector2.zero;
            cachedInput.BoostState = InputData.EBoostState.NONE;

            IsEarlyInitialized = true;
        }

        public void LateInitialize(Game game) {
            if (IsLateInitialized) return;
    
            IsLateInitialized = true;
        }      

        public void OnFly(InputAction.CallbackContext context) {                        
            var movement = context.ReadValue<Vector2>();
            cachedInput.Movement = movement;

            foreach(var input in inputs) {
                input.Notify(cachedInput);
            }
        }

        public void OnBoost(InputAction.CallbackContext context) {            
            switch(context.phase) {
                case InputActionPhase.Performed:
                case InputActionPhase.Started:                
                cachedInput.BoostState = InputData.EBoostState.PRESSED;
                // Debug.Log("started: " + context.ReadValueAsButton());
                break;                                
                case InputActionPhase.Canceled:
                cachedInput.BoostState = InputData.EBoostState.RELEASED;
                // Debug.Log("canceled: " + context.ReadValueAsButton());                
                break;
            }
            
            foreach(var input in inputs) {
                input.Notify(cachedInput);
            }
            
            if (cachedInput.BoostState != InputData.EBoostState.PRESSED) cachedInput.BoostState = InputData.EBoostState.NONE;
        }
    }
}

