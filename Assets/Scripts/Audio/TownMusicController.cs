using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Game;
using Store;
using UnityEngine;

namespace Audio {
    /// <summary>
    /// Controls the town music.
    /// </summary>
    public class TownMusicController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")] 
        [SerializeField, EventRef] private string musicEventName = "event:/Music/Home_Island_Music";
        [SerializeField, ParamRef] private string musicLevelParameter = "Home_Island_Stage";
        [SerializeField, ParamRef] private string fadeOutParameter = "Fade_Out";
        [SerializeField, Range(0.1f, 5f)] private float paramAnimationSpeed = 10f;
        [SerializeField] private Vector3 playerLevelForMusicStageChange = new Vector3(6, 12, 24);
        
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
            CheckMusicStage();
        }

        /// <summary>
        /// Checks to see if music stage need to be updated.
        /// </summary>
        private void CheckMusicStage() {
            if(GameMaster.Instance.PlayerStats.Level < playerLevelForMusicStageChange.x) {
                SetMusicStage(0f);
                return;
            }

            if(GameMaster.Instance.PlayerStats.Level < playerLevelForMusicStageChange.y) {
                SetMusicStage(1f);
                return;
            }
            
            if(GameMaster.Instance.PlayerStats.Level < playerLevelForMusicStageChange.z) {
                SetMusicStage(2f);
                return;
            }
            
            SetMusicStage(3f);
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