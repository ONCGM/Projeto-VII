using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Game;
using UnityEngine.UI;

namespace UI.Menu {
    /// <summary>
    /// Controls the loading screen behaviour.
    /// </summary>
    public class LoadingScreenController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")]
        [SerializeField, Range(0, 12)] private int menuSceneIndex = 1;
        [SerializeField, Range(0, 12)] private int gameSceneIndex = 2;
        [SerializeField, Range(0, 12)] private int loadingSceneIndex = 10;
        [SerializeField, Range(0.5f, 10f)] private float loadingSceneDelay = 5f;
        [SerializeField, Range(0.5f, 10f)] private float fadeAnimationDuration = 3f;
        [SerializeField, Range(0.2f, 3f)] private float rotateAnimationDuration = 1f;
        [SerializeField] private Vector3 scaleAnimationValue = new Vector3(0.2f, 0.2f, 0.2f);
        [SerializeField] private Vector3 rotationAnimationValue = new Vector3(0f, 0f, 360f);

        [Header("References")] 
        [SerializeField] private Transform loadingIconTransform;
        private CanvasGroup loadingGroup;

        #pragma warning restore 0649

        // Sets the class up and subs for scene events.
        private void Awake() {
            loadingGroup = GetComponent<CanvasGroup>();
            SceneManager.sceneLoaded += OnSceneLoaded;

            AnimateLoadingIcon();
            
            DOTween.To(() => loadingGroup.alpha, x => loadingGroup.alpha = x,
                       1f, fadeAnimationDuration).onComplete = () => {
                if(GameMaster.Instance.GameSceneWasLoaded) {
                    for(var i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                        if(i == loadingSceneIndex) continue;
                        if(SceneManager.GetSceneByBuildIndex(i).isLoaded) SceneManager.UnloadSceneAsync(i);
                    }
                } else {
                    SceneManager.UnloadSceneAsync(menuSceneIndex);
                }

                Invoke(nameof(LoadNextScene), loadingSceneDelay);
            };
        }

        /// <summary>
        /// Animates the loading icon.
        /// </summary>
        private void AnimateLoadingIcon() {
            loadingIconTransform.DOPunchScale(scaleAnimationValue, rotateAnimationDuration, 1, 0f);
            
            loadingIconTransform.DOLocalRotate(rotationAnimationValue, rotateAnimationDuration, RotateMode.FastBeyond360).onComplete = () => {
                if(loadingIconTransform != null) AnimateLoadingIcon();
            };
        }

        /// <summary>
        /// Loads the next scene based on the game master 'gameSceneWasLoaded';
        /// </summary>
        public void LoadNextScene() {
            if(GameMaster.Instance.GameSceneWasLoaded) {
                GameMaster.OnReturnToMenu?.Invoke();
                SceneManager.LoadSceneAsync(menuSceneIndex, LoadSceneMode.Additive);
                return;
            }
            
            SceneManager.LoadSceneAsync(gameSceneIndex, LoadSceneMode.Additive);
        }

        /// <summary>
        /// Removes the loading overlay. 
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode) {
            if(scene.buildIndex == loadingSceneIndex) return;
            if(scene.buildIndex == gameSceneIndex) FindObjectOfType<MainMenuMusicController>()?.TriggerMusicStop();
            
            DOTween.To(() => loadingGroup.alpha, x => loadingGroup.alpha = x, 0f, fadeAnimationDuration).onComplete =
                () => {
                    SceneManager.UnloadSceneAsync(loadingSceneIndex);
                    DOTween.Kill(loadingIconTransform);
                };
        }
    }
}