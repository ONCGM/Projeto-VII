using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI.Localization;
using DG.Tweening;
using Localization;
using TMPro;
using UnityEngine;

namespace UI.Popups {
    /// <summary>
    /// Pops up a animated canvas with a text to display the name of the location.
    /// </summary>
    public class CanvasPopupLocationName : MonoBehaviour {
        #pragma warning disable 0649
        
        [Header("Settings")] 
        [SerializeField, Range(0.1f, 5f)] private float animationInTime = 1f;
        [SerializeField, Range(0.1f, 5f)] private float animationOutDelayTime = 1f;
        [SerializeField, Range(0.1f, 5f)] private float animationOutTime = 2f;
        
        // Components.
        private TextMeshLocalizer localizer;
        private TMP_Text textMesh;
        private CanvasGroup canvasGroup;
        
        #pragma warning restore 0649

        // Gets the necessary references.
        private void Awake() {
            localizer = GetComponentInChildren<TextMeshLocalizer>();
            textMesh = GetComponentInChildren<TMP_Text>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// Sets the needed values for the canvas to function, then triggers the start of the animation.
        /// </summary>
        public void SetUpLocationCanvas(string locationKey) {
            localizer.UpdateKey(locationKey);
            AnimateIn();
        }
        
        /// <summary>
        /// Sets the needed values for the canvas to function, then triggers the start of the animation.
        /// </summary>
        public void SetUpLocationCanvas(IEnumerable<string> keys) {
            localizer.enabled = false;
            if(LocalizationSystem.CurrentLanguage == LocalizationSystem.Language.Japanese) {
                textMesh.text = keys.Aggregate(string.Empty,
                                               (current, key) =>
                                                   $"{current} {LocalizationSystem.GetLocalizedValue(key)}");
            } else {
                textMesh.text = keys.Aggregate(string.Empty,
                                               (current, key) =>
                                                   $"{current} {LocalizationSystem.GetLocalizedValue(key)}");
            }

            AnimateIn();
        }

        /// <summary>
        /// Animates in the canvas, waits the specified time and then triggers the exit animation.
        /// </summary>
        [ContextMenu("Test Animation")]
        private void AnimateIn() {
            DOVirtual.Float(0f, 1f, animationInTime, value => canvasGroup.alpha = value).onComplete += () => {
                Invoke(nameof(AnimateOut), animationOutDelayTime);
            };
        }

        /// <summary>
        /// Animates the canvas out and then destroys it.
        /// </summary>
        private void AnimateOut() {
            DOVirtual.Float(1f, 0f, animationOutTime, value => canvasGroup.alpha = value).onComplete += () => {
                Destroy(gameObject);
            };
        }
    }
}