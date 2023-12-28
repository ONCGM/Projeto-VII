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
        /// <summary>
        /// The text mesh component of this localizer.
        /// </summary>
        public TMP_Text TextMeshComponent { get; private set; }

        // Sets up the component and loads the text.
        private void Awake() {
            TextMeshComponent = GetComponent<TMP_Text>();
            TextMeshComponent.text = localizedText.value;

            LocalizationSystem.OnLanguageUpdate += UpdateText;
        }

        /// <summary>
        /// Updates the text when the language has changed.
        /// </summary>
        private void UpdateText() {
            TextMeshComponent.text = localizedText.value;
        }
        
        /// <summary>
        /// Updates the text when the key has changed.
        /// </summary>
        public void UpdateKey(string key) {
            localizedText = new LocalizedString(key);
            TextMeshComponent.text = localizedText.value;
        }

        // <summary>
        // Removes the subscription from the action.
        // </summary>
        private void OnDestroy() => LocalizationSystem.OnLanguageUpdate -= UpdateText;
    }
}