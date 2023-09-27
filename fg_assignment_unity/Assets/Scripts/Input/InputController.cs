using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Data.Common;

namespace Lander {
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

    public class InputController : MonoBehaviour
    {
        private IInput[] inputs;

        public void Initialize() {
            inputs = FindObjectsOfType<MonoBehaviour>().OfType<IInput>().ToArray();
        }

        public void OnFly(InputAction.CallbackContext context) {
            var data = new InputData();
            data.Movement = Vector3.zero;
            data.BoostState = InputData.EBoostState.NONE;

            var movement = context.ReadValue<Vector2>();
            data.Movement = movement;
            data.BoostState = InputData.EBoostState.NONE;

            foreach(var input in inputs) {
                input.Notify(data);
            }
        }

        public void OnBoost(InputAction.CallbackContext context) {
            var data = new InputData();
            data.Movement = Vector3.zero;
            data.BoostState = InputData.EBoostState.NONE;

            switch(context.phase) {
                case InputActionPhase.Performed:
                case InputActionPhase.Started:
                data.BoostState = InputData.EBoostState.PRESSED;
                // Debug.Log("started: " + context.ReadValueAsButton());
                break;                                
                case InputActionPhase.Canceled:
                data.BoostState = InputData.EBoostState.RELEASED;
                // Debug.Log("canceled: " + context.ReadValueAsButton());                
                break;
            }
            
            foreach(var input in inputs) {
                input.Notify(data);
            }
        }   
    }
}

