using System;
using System.Collections;
using System.Collections.Generic;
using Localization;
using TMPro;
using UnityEngine;

namespace UI.Localization {
    /// <summary>
    /// Localizes a TextMeshPro text using a LocalizedString.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TextMeshLocalizer : MonoBehaviour {
        [Header("Settings")] 
        [SerializeField] private LocalizedString localizedText;

        // Components.
        private TMP_Text textMesh;
        
        // Sets up the component and loads the text.
        private void Awake() {
            textMesh = GetComponent<TMP_Text>();
            var value = LocalizationSystem.GetLocalizedValue(localizedText.value);
            textMesh.text = value;

            LocalizationSystem.OnLanguageUpdate += UpdateText;
        }

        /// <summary>
        /// Updates the text when the language or key has changed.
        /// </summary>
        private void UpdateText() {
            var value = LocalizationSystem.GetLocalizedValue(localizedText.value);
            textMesh.text = value;
        }
        
        /// <summary>
        /// Updates the text when the language or key has changed.
        /// </summary>
        public void UpdateText(string key) {
            localizedText.key = key;
            var value = LocalizationSystem.GetLocalizedValue(localizedText.value);
            textMesh.text = value;
        }

        // <summary>
        // Removes the subscription from the action.
        // </summary>
        // private void OnDestroy() => LocalizationSystem.OnLanguageUpdate -= UpdateText;
    }
}