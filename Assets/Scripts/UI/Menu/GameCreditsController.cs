using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Menu {
    /// <summary>
    /// Controls the behaviour of the game credits scene.
    /// </summary>
    public class GameCreditsController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")] 
        [SerializeField, Range(0, 12)] private int gameSceneIndex = 2;
        [SerializeField, Range(0, 12)] private int menuSceneIndex = 1;
        [SerializeField, Range(0, 12)] private int creditsSceneIndex = 9;
        [SerializeField, Range(0.5f, 10f)] private float fadeAnimationDuration = 3f;
        [SerializeField, Range(30f, 350f)] private float creditsAnimationDuration = 60f;
        [SerializeField] private float creditsAnimationFinalPosition = 2000f;

        [Header("References")] 
        [SerializeField] private RectTransform creditsHolderRect;
        private CanvasGroup creditsGroup;
        
        // Events.
        public static Action OnCreditsOpen;
        
        #pragma warning restore 0649

        // Sets the class up and subs for scene events.
        private void Awake() {
            creditsGroup = GetComponent<CanvasGroup>();
            SceneManager.sceneLoaded += OnSceneLoaded;

            creditsHolderRect.DOLocalMoveY(creditsAnimationFinalPosition, creditsAnimationDuration).OnComplete(GoBackToLastScene).id = "credits";

            OnCreditsOpen?.Invoke();
            
            DOTween.To(() => creditsGroup.alpha, x => creditsGroup.alpha = x, 1f, fadeAnimationDuration).onComplete =
                () => {
                    for(var i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                        if(i == creditsSceneIndex) continue;
                        if(SceneManager.GetSceneByBuildIndex(i).isLoaded) SceneManager.UnloadSceneAsync(i);
                    }
                };
        }

        /// <summary>
        /// Goes back to the last loaded scene.
        /// </summary>
        public void GoBackToLastScene() {
            if(GameMaster.Instance.GameSceneWasLoaded) {
                SceneManager.LoadSceneAsync(gameSceneIndex, LoadSceneMode.Additive);
                return;
            }

            SceneManager.LoadSceneAsync(menuSceneIndex, LoadSceneMode.Additive);
        }
        
        /// <summary>
        /// Removes the credits overlay. 
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode) {
            if(scene.buildIndex == creditsSceneIndex) return;
            
            DOTween.To(() => creditsGroup.alpha, x => creditsGroup.alpha = x, 0f, fadeAnimationDuration).onComplete =
                () => {
                    DOTween.Kill("credits");
                    SceneManager.UnloadSceneAsync(creditsSceneIndex);
                    DOTween.Kill(gameObject);
                };
        }
    }
}