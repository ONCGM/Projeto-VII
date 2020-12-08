using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Entity.Player;
using FMOD.Studio;
using FMODUnity;
using Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Audio {
    /// <summary>
    /// Controls the wave sounds.
    /// </summary>
    public class WavesAmbienceController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")] 
        [SerializeField, EventRef] private string musicEventName = "event:/SFX/Ambience_Events/Beach_Waves";

        [SerializeField, ParamRef] private string beachLevelParameter = "Beach_Stage";
        [SerializeField, ParamRef] private string beachVolumeParameter = "Beach_Volume";
        [SerializeField, ParamRef] private string fadeOutParameter = "Fade_Out";
        [SerializeField, Range(0.1f, 5f)] private float paramAnimationSpeed = 7f;
        [SerializeField] private Transform pierPosition;
        [SerializeField, Range(0f, 3f)] private float nearIslandPierValue = 1.8f;
        [SerializeField, Range(0f, 3f)] private float nearPierValue = 3f;
        [SerializeField, Range(1f, 100f)] private float pierMaxDistance = 75f;
        [SerializeField] private bool isTown = true;
        
        [Header("Initial Values")] 
        [SerializeField, Range(0f, 3f)] private float startingMusicLevel;

        [SerializeField, Range(0f, 1f)] private float startingFadeOutLevel;

        private EventInstance instance;
        private PlayerController player; 
        #pragma warning restore 0649

        // Sets up the class and start FMOD Event Instance.
        private void Awake() {
            DontDestroyOnLoad(gameObject);
            if(pierPosition == null) transform.GetChild(0);
            
            instance = RuntimeManager.CreateInstance(musicEventName);
            instance.start();
            instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject.transform));
    
            instance.setParameterByName(beachLevelParameter, startingMusicLevel);
            instance.setParameterByName(fadeOutParameter, startingFadeOutLevel);

            InvokeRepeating(nameof(CheckEventStage), 0f, 7f);
        }

        /// <summary>
        /// Checks to see if music stage need to be updated.
        /// </summary>
        private void CheckEventStage() {
            if(player == null) {
                player = FindObjectOfType<PlayerController>();
                return;
            }
            var playerPos = player.transform.position;
            var pierPos = pierPosition.position;
            
            SetEventVolume(Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, pierMaxDistance, Vector3.Distance(playerPos, pierPos))));
            SetEventStage(Mathf.Lerp(0f, isTown ? nearPierValue : nearIslandPierValue, Mathf.InverseLerp(0f, pierMaxDistance, Vector3.Distance(playerPos, pierPos))));

            SceneManager.sceneUnloaded += arg0 => TriggerEventStop();
        }

        /// <summary>
        /// Changes the current level of the main menu music.
        /// </summary>
        public void SetEventStage(float value) {
            DOTween.To(() => (float) instance.getParameterByName(beachLevelParameter, out var param),
                       param => instance.setParameterByName(beachLevelParameter, param), value, paramAnimationSpeed);
        }
        
        /// <summary>
        /// Changes the current level of the main menu music.
        /// </summary>
        public void SetEventVolume(float value) {
            DOTween.To(() => (float) instance.getParameterByName(beachVolumeParameter, out var param),
                       param => instance.setParameterByName(beachVolumeParameter, param), value, paramAnimationSpeed);
        }

        /// <summary>
        /// Stops the main menu music.
        /// Also, destroys this Game Object.
        /// </summary>
        public void TriggerEventStop() {
            DOTween.To(() => (float) instance.getParameterByName(fadeOutParameter, out var param),
                       param => instance.setParameterByName(fadeOutParameter, param), 1f, 1f).onComplete += () => Destroy(gameObject);
        }

        // Releases FMOD resources.
        private void OnDestroy() {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
        }
    }
}