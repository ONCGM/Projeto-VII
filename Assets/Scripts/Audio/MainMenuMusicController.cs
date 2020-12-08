using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

namespace Audio {
    /// <summary>
    /// Controls the main menu music and fades it
    /// after loading the game scene.
    /// </summary>
    public class MainMenuMusicController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")] 
        [SerializeField, EventRef] private string musicEventName = "event:/Music/Main_Menu_Music";
        [SerializeField, ParamRef] private string musicLevelParameter = "Main_Menu_Stage";
        [SerializeField, ParamRef] private string fadeOutParameter = "Fade_Out";
        [SerializeField, Range(0.1f, 5f)] private float paramAnimationSpeed = 1f;
        [SerializeField, Range(10f, 30f)] private float timeUntilDestruction = 10f;

        [Header("Initial Values")]
        [SerializeField, Range(0f, 3f)] private float startingMusicLevel;
        [SerializeField, Range(0f, 1f)] private float startingFadeOutLevel;
        
        private EventInstance instance;
        #pragma warning restore 0649
        
        // Sets up the class and start FMOD Event Instance.
        private void Awake() {
            DontDestroyOnLoad(gameObject);
            instance = RuntimeManager.CreateInstance(musicEventName);
            instance.start();

            instance.setParameterByName(musicLevelParameter, startingMusicLevel);
            instance.setParameterByName(fadeOutParameter, startingFadeOutLevel);
        }

        /// <summary>
        /// Changes the current level of the main menu music.
        /// </summary>
        public void SetMusicStage(float value) {
            DOTween.To(() => (float) instance.getParameterByName(musicLevelParameter, out var param), 
                       param => instance.setParameterByName(musicLevelParameter, param), value, paramAnimationSpeed);
        }

        /// <summary>
        /// Stops the main menu music.
        /// Also, destroys this Game Object.
        /// </summary>
        public void TriggerMusicStop() {
            DOTween.To(() => (float) instance.getParameterByName(fadeOutParameter, out var param), 
                       param => instance.setParameterByName(fadeOutParameter, param), 1f, timeUntilDestruction - 1f)
                   .onComplete = () => Destroy(gameObject);
        }

        // Releases FMOD resources.
        private void OnDestroy() {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
        }
    }
}