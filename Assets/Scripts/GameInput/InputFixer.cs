using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace GameInput {
    /// <summary>
    /// A bodge to deal with Unity's faulty input module.
    /// Hope that it works, else the game is broken.
    /// </summary>
    public class InputFixer : MonoBehaviour {
        // Setups.
        private void Awake() {
            InvokeRepeating(nameof(ResetInputModule), 1f, 2f);
        }

        // Cancels.
        private void OnDestroy() {
            CancelInvoke(nameof(ResetInputModule));
        }

        /// <summary>
        /// Resets and reactivates the module.
        /// </summary>
        private void ResetInputModule() {
            foreach(var inputModule in FindObjectsOfType<InputSystemUIInputModule>()) {
                inputModule.enabled = false;
                inputModule.UpdateModule();
                inputModule.enabled = true;
                inputModule.DeactivateModule();
                inputModule.ActivateModule();
                inputModule.UpdateModule();
            }
        }
    }
}