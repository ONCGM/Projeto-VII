using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace UI {
    /// <summary>
    /// Animates the loading icon and deletes itself after a while.
    /// </summary>
    public class SavingUiCanvas : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")]
        [SerializeField, Range(0.1f, 3f)] private float fadeAnimationTime = 0.5f;
        [SerializeField, Range(0.3f, 3f)] private float animationTime = 1f;
        [SerializeField, Range(1, 10)] private int timesToSpin = 5;
        [SerializeField] private Vector3 scaleAnimationValue = new Vector3(0.2f, 0.2f, 0.2f);
        [SerializeField] private Vector3 rotationAnimationValue = new Vector3(0f, 0f, 360f);

        private CanvasGroup canvasGroup;
        #pragma warning restore 0649

        // Setup.
        private void Awake() {
            canvasGroup = GetComponent<CanvasGroup>();
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, fadeAnimationTime);
            AnimateIcon();
        }

        /// <summary>
        /// Rotates the image.
        /// </summary>
        private void AnimateIcon() {
            transform.GetChild(0).DOPunchScale(scaleAnimationValue, animationTime, 1, 0f);
            
            transform.GetChild(0).DOLocalRotate(rotationAnimationValue, animationTime, RotateMode.FastBeyond360).onComplete = () => {
                if(timesToSpin > 0) {
                    if(transform.GetChild(0).gameObject != null) AnimateIcon();
                    else DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, fadeAnimationTime).onComplete += () => Destroy(gameObject);
                } else {
                    DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, fadeAnimationTime).onComplete += () => Destroy(gameObject);
                }

                timesToSpin--;
            };
        }
    }
}