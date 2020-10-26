using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI.Menu {
    /// <summary>
    /// Unlocks the continue button if the player has a valid save.
    /// </summary>
    public class UnlockContinueButton : MonoBehaviour {
        private Button continueButton;

        // Checks if the button should be unlocked
        private void Start() {
            continueButton = GetComponent<Button>();
            continueButton.interactable = !SaveSystem.LoadedData.brandSpankingNewSave;
        }
    }
}