using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Entity.NPC;
using FMOD.Studio;
using FMODUnity;
using Store;
using UnityEngine;

namespace Audio {
    /// <summary>
    /// Controls the store music.
    /// </summary>
    public class StoreMusicController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")] 
        [SerializeField, EventRef] private string musicEventName = "event:/Music/Store_Music";
        [SerializeField, ParamRef] private string musicLevelParameter = "Store_Stage";
        [SerializeField, ParamRef] private string fadeOutParameter = "Fade_Out";
        [SerializeField, Range(0.1f, 5f)] private float paramAnimationSpeed = 10f;
        [SerializeField] private Vector2 npcCountForMusicStageChange = new Vector2(3, 6);
        
        [Header("Initial Values")]
        [SerializeField, Range(0f, 3f)] private float startingMusicLevel;
        [SerializeField, Range(0f, 1f)] private float startingFadeOutLevel;
        
        private EventInstance instance;
        private StoreController store;
        #pragma warning restore 0649
        
        // Sets up the class and start FMOD Event Instance.
        private void Awake() {
            DontDestroyOnLoad(gameObject);
            store = FindObjectOfType<StoreController>();
            instance = RuntimeManager.CreateInstance(musicEventName);
            instance.start();
        }

        // Sets the params.
        private void Start() {
            instance.setParameterByName(musicLevelParameter, startingMusicLevel);
            instance.setParameterByName(fadeOutParameter, startingFadeOutLevel);
            
            InvokeRepeating(nameof(CheckMusicStage),3.5f, 3.5f);
        }

        /// <summary>
        /// Checks to see if music stage need to be updated.
        /// </summary>
        private void CheckMusicStage() {
            if(store == null) {
                store = FindObjectOfType<StoreController>();
                return;
            }
            
            var npcCount = FindObjectsOfType<NpcController>().Length;
            
            if(npcCount >= npcCountForMusicStageChange.x &&
               npcCount < npcCountForMusicStageChange.y) {
                SetMusicStage(2f);
                return;
            }

            if(npcCount >= npcCountForMusicStageChange.y) {
                SetMusicStage(3f);
                return;
            }

            if(store.StoreOpen) {
                SetMusicStage(1f);
                return;
            }
            
            SetMusicStage(0f);
        }

        /// <summary>
        /// Changes the current level of the main menu music.
        /// </summary>
        public void SetMusicStage(float value) {
            DOTween.To(() => {
                           instance.getParameterByName(musicLevelParameter, out var param); 
                           return param;
                       }, 
                       param => instance.setParameterByName(musicLevelParameter, param), value, paramAnimationSpeed);
        }

        /// <summary>
        /// Stops the main menu music.
        /// Also, destroys this Game Object.
        /// </summary>
        public void TriggerMusicStop() {
            CancelInvoke(nameof(CheckMusicStage));
            StartCoroutine(nameof(MusicStop));
            DOTween.To(() => (float) instance.getParameterByName(fadeOutParameter, out var param),
                       param => instance.setParameterByName(fadeOutParameter, param), 1f, 1f);
        }

        /// <summary>
        /// Stops the music.
        /// </summary>
        private IEnumerator MusicStop() {
            yield return new WaitUntil(() => {
                instance.getPlaybackState(out var state);
                return state == PLAYBACK_STATE.STOPPED;
            });
            
            Destroy(gameObject);
        }

        // Releases FMOD resources.
        private void OnDestroy() {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
        }
    }
}