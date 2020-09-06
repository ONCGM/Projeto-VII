using System;
using System.Collections;
using System.Collections.Generic;
using Localization;
using TMPro;
using UnityEngine;

namespace UI.Localization {
    /// <summary>
    /// Localizes a TextMeshPro text using a key.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TextMeshLocalizer : MonoBehaviour {
        [Header("Settings")] 
        [SerializeField] private string key = "placeholder";

        // Components.
        private TMP_Text textMesh;
        
        // Sets up the component and loads the text.
        private void Awake() {
            textMesh = GetComponent<TMP_Text>();
            textMesh.text = LocalizationSystem.GetLocalizedValue(key);

            LocalizationSystem.OnLanguageUpdate += UpdateText;
        }

        /// <summary>
        /// Updates the text when the language has changed.
        /// </summary>
        private void UpdateText() {
            textMesh.text = LocalizationSystem.GetLocalizedValue(key);
        }

        // <summary>
        // Removes the subscription from the action.
        // </summary>
        // private void OnDestroy() => LocalizationSystem.OnLanguageUpdate -= UpdateText;
    }
}