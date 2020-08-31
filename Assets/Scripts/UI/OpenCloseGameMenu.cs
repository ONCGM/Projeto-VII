using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI {
    [RequireComponent(typeof(Canvas))]
    public class OpenCloseGameMenu : MonoBehaviour {
        private ActionInputs inputs;
        private bool isOpen { get; set; }
        private Canvas gameMenuCanvas;

        private void Awake() {
            inputs = new ActionInputs();
            inputs.Player.Menu.performed += OpenCloseMenu;
            gameMenuCanvas = GetComponentInParent<Canvas>();
        }

        private void OpenCloseMenu(InputAction.CallbackContext callbackContext) {
            isOpen = !isOpen;
            gameMenuCanvas.enabled = isOpen;
        }
        
        // OnEnable Unity Event, enables input.
        private void OnEnable() {
            inputs.Enable();
        }

        // OnDisable Unity Event, disables input.
        private void OnDisable() {
            inputs.Disable();
        }

        // Disposes and disables the input.
        private void OnDestroy() {
            inputs.Player.Menu.performed -= OpenCloseMenu;
            inputs.Dispose();
        }
    }
}