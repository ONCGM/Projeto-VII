using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using Islands;
using UnityEngine;

namespace UI.Menu {
    /// <summary>
    /// Displays the results of the player exploration.
    /// </summary>
    public class CombatDebriefUi : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")] [SerializeField, Range(0.1f, 3f)]
        private float popinAnimationDuration = 0.75f;
        
        // Components.
        private CanvasGroup canvasGroup;
        
        #pragma warning restore 0649

        // Animates in the canvas and sets up the values.
        private void Awake() {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            DOTween.To(()=> canvasGroup.alpha, x=> canvasGroup.alpha = x, 1f, popinAnimationDuration);
            GameMaster.Instance.GameState = ExecutionState.PopupPause;
        }

        /// <summary>
        /// Closes the canvas and allows the player to keep exploring.
        /// </summary>
        public void KeepExploring() {
            FindObjectOfType<IslandsSceneTransition>().startedTransition = false;
            CloseCanvas();
        }

        /// <summary>
        /// Closes the canvas and starts the ship routine to travel back to town.
        /// </summary>
        public void ReturnToTown() {
            GameMaster.Instance.ShipTravel.StartTravelToTown();
            CloseCanvas();
        }

        /// <summary>
        /// Animates out the canvas and destroys it.
        /// </summary>
        private void CloseCanvas() {
            GameMaster.Instance.GameState = ExecutionState.Normal;
            DOTween.To(()=> canvasGroup.alpha, x=> canvasGroup.alpha = x, 0f, popinAnimationDuration).onComplete +=
                () => {
                    Destroy(gameObject);
                };
        }
    }
}