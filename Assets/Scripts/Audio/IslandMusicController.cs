using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Entity.Enemies;
using Entity.Player;
using FMOD.Studio;
using FMODUnity;
using Store;
using UnityEngine;

namespace Audio {
    /// <summary>
    /// Controls the island music based on player stats and enemy count.
    /// </summary>
    public class IslandMusicController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")] 
        [SerializeField, EventRef] private string musicEventName = "event:/Music/Adventure_Island_Music";
        [SerializeField, ParamRef] private string musicLevelParameter = "Adventure_Island_Stage";
        [SerializeField, ParamRef] private string fadeOutParameter = "Fade_Out";
        [SerializeField, Range(0.1f, 5f)] private float paramAnimationSpeed = 10f;
        [SerializeField] private Vector4 enemyCountForMusicStageChange = new Vector4(1, 3, 6, 9);
        [SerializeField, Range(0.05f, 0.5f)] private float criticalHealthMultiplier = 0.2f;
        [SerializeField, Range(1f, 25f)] private float enemyDetectionRadius = 15f;
        [SerializeField] private LayerMask enemyLayer;

        [Header("Initial Values")] [SerializeField, Range(0f, 3f)]
        private float startingMusicLevel;

        [SerializeField, Range(0f, 1f)] private float startingFadeOutLevel;

        private EventInstance instance;
        private PlayerController player;
        private int enemyCount = 0;
        #pragma warning restore 0649

        // Sets up the class and start FMOD Event Instance.
        private void Awake() {
            DontDestroyOnLoad(gameObject);
            player = FindObjectOfType<PlayerController>();
            instance = RuntimeManager.CreateInstance(musicEventName);
            instance.start();

            instance.setParameterByName(musicLevelParameter, startingMusicLevel);
            instance.setParameterByName(fadeOutParameter, startingFadeOutLevel);
            
            InvokeRepeating(nameof(CheckMusicStage),10f, 2f);
        }

        /// <summary>
        /// Checks to see if music stage need to be updated.
        /// </summary>
        private void CheckMusicStage() {
            if(player == null) {
                player = FindObjectOfType<PlayerController>();
                return;
            }

            if(player.Health < player.MaxHealth * criticalHealthMultiplier) {
                SetMusicStage(6f);
                return;
            }

            transform.position = player.transform.position;

            var results = new RaycastHit[]{};
            Physics.SphereCastNonAlloc(transform.position, enemyDetectionRadius, Vector3.one, results, Mathf.Infinity, enemyLayer);


            enemyCount = results.ToList<RaycastHit>().FindAll(x => x.transform.gameObject.GetComponent<Enemy>().IsDead == false).Count;

            if(enemyCount <= 0) {
                SetMusicStage(0f);
                return;
            }
            
            if(enemyCount <= enemyCountForMusicStageChange.x) {
                SetMusicStage(1f);
                return;
            }

            if(enemyCount <= enemyCountForMusicStageChange.y) {
                SetMusicStage(2f);
                return;
            }

            if(enemyCount <= enemyCountForMusicStageChange.z) {
                SetMusicStage(3f);
                return;
            }
            
            if(enemyCount <= enemyCountForMusicStageChange.w) {
                SetMusicStage(4f);
                return;
            }
            
            SetMusicStage(5f);
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