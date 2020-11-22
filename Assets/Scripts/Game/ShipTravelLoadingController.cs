using System.Collections;
using System.Collections.Generic;
using Audio;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Game {
    /// <summary>
    /// Controls the ship travel loading screen behaviour.
    /// </summary>
    public class ShipTravelLoadingController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")]
        [SerializeField, Range(0, 12)] private int townSceneIndex = 3;
        [SerializeField, Range(0, 12)] private int islandsSceneIndex = 5;
        [SerializeField, Range(0, 12)] private int loadingSceneIndex = 4;
        [SerializeField, Range(0.5f, 10f)] private float loadingSceneDelay = 5f;
        [SerializeField, Range(0.5f, 10f)] private float fadeAnimationDuration = 3f;
        [SerializeField, Range(0.2f, 3f)] private float rotateAnimationDuration = 1f;
        [SerializeField] private Vector3 scaleAnimationValue = new Vector3(0.2f, 0.2f, 0.2f);
        [SerializeField] private Vector3 rotationAnimationValue = new Vector3(0f, 0f, 360f);

        [Header("References")] 
        [SerializeField] private Transform loadingIconTransform;
        private CanvasGroup loadingGroup;
        private bool loadingTown;
        private bool triggeredRemoval;

        #pragma warning restore 0649

        // Sets the class up and subs for scene events.
        private void Awake() {
            loadingGroup = GetComponent<CanvasGroup>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// Triggers the loading screen animation.
        /// </summary>
        public void StartLoadingSequence(int sceneIndexToLoad) {
            AnimateLoadingIcon();
            
            if(sceneIndexToLoad == townSceneIndex) {
                loadingTown = true;
            } else if(sceneIndexToLoad == islandsSceneIndex) {
                loadingTown = false;
            }

            DOTween.To(() => loadingGroup.alpha, x => loadingGroup.alpha = x,
                       1f, fadeAnimationDuration).onComplete = LoadNextScene;
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
        /// Loads the next scene based on the parameters.
        /// </summary>
        public void LoadNextScene() {
            if(loadingTown) {
                SceneManager.LoadSceneAsync(townSceneIndex, LoadSceneMode.Additive);
                GameMaster.Instance.ShipTravel.IslandLoader.UnloadIslands();
                GameMaster.Instance.ShipTravel.IslandLoader = null;
                SceneManager.UnloadSceneAsync(islandsSceneIndex);
                return;
            }
            
            SceneManager.LoadSceneAsync(islandsSceneIndex, LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync(townSceneIndex);
        }

        /// <summary>
        /// Removes the loading overlay. 
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode) {
            if(scene.buildIndex == loadingSceneIndex) return;
            if(scene.buildIndex == islandsSceneIndex) GameMaster.Instance.ShipTravel.LoadIsland();
            if(scene.buildIndex == townSceneIndex) GameMaster.Instance.ShipTravel.LoadTown();

            if(!triggeredRemoval) RemoveLoadingOverlay();
        }
        
        /// <summary>
        /// Removes the loading scene and overlay.
        /// </summary>
        private void RemoveLoadingOverlay() {
            triggeredRemoval = true;
            DOTween.To(() => loadingGroup.alpha, x => loadingGroup.alpha = x,
                       0.999f, fadeAnimationDuration * 0.4f).onComplete =
                () => {
                    DOTween.To(() => loadingGroup.alpha, x => loadingGroup.alpha = x, 
                               0f, fadeAnimationDuration).onComplete =
                        () => {
                            SceneManager.UnloadSceneAsync(loadingSceneIndex);
                            DOTween.Kill(loadingIconTransform);
                        };
                };
        }
    }
}