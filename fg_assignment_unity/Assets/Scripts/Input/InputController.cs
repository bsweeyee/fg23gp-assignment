using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Data.Common;

namespace Lander {
    public struct InputData {
        public Vector2 Movement;
        public bool Boost;
    }

    public interface IInput {
        public void Notify(InputData data);
    }

    public class InputController : MonoBehaviour
    {
        private InputData data;
        private IInput[] inputs;

        public void Initialize() {
            inputs = FindObjectsOfType<MonoBehaviour>().OfType<IInput>().ToArray();
        }

        public void OnFly(InputAction.CallbackContext context) {
            var movement = context.ReadValue<Vector2>();
            data.Movement = movement;

            foreach(var input in inputs) {
                input.Notify(data);
            }
        }

        public void OnBoost(InputAction.CallbackContext context) {
            switch(context.phase) {
                case InputActionPhase.Started:                
                data.Boost = context.ReadValueAsButton();
                break;
                case InputActionPhase.Performed:
                break;
                case InputActionPhase.Canceled:
                data.Boost = context.ReadValueAsButton();
                break;
            }
            
            foreach(var input in inputs) {
                input.Notify(data);
            }
        }   
    }
}

