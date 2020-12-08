using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
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
        [Header("FMOD Values")]
        [SerializeField, Range(0f, 3f)] private float startingMusicLevel;
        [SerializeField, Range(0f, 1f)] private float startingFadeOutLevel;
        [SerializeField, Range(0.1f, 15f)] private float paramAnimationSpeed = 10f;
        [SerializeField, Range(20f, 350f)] private float musicFadeOutOffset = 32f;
        [SerializeField, EventRef] private string musicEventName = "event:/Music/Main_Menu_Music";
        [SerializeField, ParamRef] private string musicLevelParameter = "Main_Menu_Stage";
        [SerializeField, ParamRef] private string fadeOutParameter = "Fade_Out";

        [Header("References")] 
        [SerializeField] private RectTransform creditsHolderRect;
        private CanvasGroup creditsGroup;
        private EventInstance instance;
        private bool loadingStarted;
        private bool allowSkip;
        
        #pragma warning restore 0649

        // Sets the class up and subs for scene events.
        private void Awake() {
            creditsGroup = GetComponent<CanvasGroup>();
            SceneManager.sceneLoaded += OnSceneLoaded;

            creditsHolderRect.DOLocalMoveY(creditsAnimationFinalPosition, creditsAnimationDuration).OnComplete(GoBackToLastScene).SetEase(Ease.Linear).id = "credits";
            
            Invoke(nameof(FadeMusic), (creditsAnimationDuration - musicFadeOutOffset));
            
            instance = RuntimeManager.CreateInstance(musicEventName);
            instance.start();

            instance.setParameterByName(musicLevelParameter, startingMusicLevel);
            instance.setParameterByName(fadeOutParameter, startingFadeOutLevel);
            SetMusicStage(3f);
            
            DOTween.To(() => (float) instance.getParameterByName(fadeOutParameter, out var param),
                       param => instance.setParameterByName(fadeOutParameter, param), 0f, fadeAnimationDuration);

            GameMaster.OnCreditsOpen?.Invoke();
            
            DOTween.To(() => creditsGroup.alpha, x => creditsGroup.alpha = x, 1f, fadeAnimationDuration).onComplete =
                () => {
                    for(var i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                        if(i == creditsSceneIndex) continue;
                        if(SceneManager.GetSceneByBuildIndex(i).isLoaded) SceneManager.UnloadSceneAsync(i);
                    }

                    allowSkip = true;
                };
        }

        /// <summary>
        /// Fades the credits music.
        /// </summary>
        private void FadeMusic() {
            instance.setParameterByName(fadeOutParameter, 0.12f);
        }
        
        /// <summary>
        /// Stops the main menu music.
        /// Also, destroys this Game Object.
        /// </summary>
        public void TriggerMusicStop() {
            DOTween.To(() => (float) instance.getParameterByName(fadeOutParameter, out var param),
                       param => instance.setParameterByName(fadeOutParameter, param), 1f, fadeAnimationDuration - 1f);
        }
        
        /// <summary>
        /// Changes the current level of the main menu music.
        /// </summary>
        public void SetMusicStage(float value) {
            DOTween.To(() => (float) instance.getParameterByName(musicLevelParameter, out var param), 
                       param => instance.setParameterByName(musicLevelParameter, param), value, paramAnimationSpeed);
        }

        /// <summary>
        /// Goes back to the last loaded scene.
        /// </summary>
        public void GoBackToLastScene() {
            if(loadingStarted) return;
            // if(GameMaster.Instance.GameSceneWasLoaded) {
            //     SceneManager.LoadSceneAsync(gameSceneIndex, LoadSceneMode.Additive);
            //     return;
            // }

            
            if(!allowSkip) return;
            SceneManager.LoadSceneAsync(menuSceneIndex, LoadSceneMode.Additive);
            loadingStarted = true;
        }
        
        /// <summary>
        /// Removes the credits overlay. 
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode) {
            if(scene.buildIndex == creditsSceneIndex) return;
            
            TriggerMusicStop();
            DOTween.To(() => creditsGroup.alpha, x => creditsGroup.alpha = x, 0f, fadeAnimationDuration).onComplete =
                () => {
                    DOTween.Kill("credits");
                    SceneManager.UnloadSceneAsync(creditsSceneIndex);
                    DOTween.Kill(gameObject);
                };
        }
        
        // Releases FMOD resources.
        private void OnDestroy() {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
        }
    }
}