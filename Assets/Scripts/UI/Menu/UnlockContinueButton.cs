using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu {
    /// <summary>
    /// Unlocks the continue button if the player has a valid save.
    /// </summary>
    public class UnlockContinueButton : MonoBehaviour {
        private Button continueButton;

        // Checks if the button should be unlocked
        private void Start() {
            continueButton = GetComponent<Button>();
            continueButton.interactable = (true); // TODO: Add integration with save system.
        }
    }
}