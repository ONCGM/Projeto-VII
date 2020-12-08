using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using DG.Tweening;
using Entity.Player;
using FMOD.Studio;
using FMODUnity;
using Game;
using Items;
using TMPro;
using UI.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility;

namespace UI.Menu {
    /// <summary>
    /// Displays the end game scene and later on the credits.
    /// Deletes player progress.
    /// </summary>
    public class EndGameController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")] 
        [SerializeField] private float fadeAnimationDuration = 3f;
        [SerializeField] private float textFadeAnimationDuration = 0.7f;
        [SerializeField, Range(2000, 20000)] private int finalObjectiveAmount = 9999;
        [SerializeField] private int creditsSceneIndex = 9;

        [Header("Components")] 
        [SerializeField] private TMP_Text mainText;
        [SerializeField] private CanvasGroup mainCanvasGroup;
        [SerializeField] private CanvasGroup buttonCanvasGroup;
        
        [Header("Localization")] 
        [SerializeField] private LocalizedString tenthDayKey;
        [SerializeField] private LocalizedString finalObjectiveKey, 
                                                 totalCoinsKey,
                                                 coinsKey,
                                                 successKey,
                                                 failKey;

        private WaitForSeconds waitForSeconds;
        #pragma warning restore 0649

        // Setup.
        private void Awake() {
            GameMaster.OnCreditsOpen?.Invoke();
            mainCanvasGroup = GetComponent<CanvasGroup>();
            waitForSeconds = new WaitForSeconds(textFadeAnimationDuration);

            if(FindObjectOfType<StoreMusicController>()) FindObjectOfType<StoreMusicController>().TriggerMusicStop();
            if(FindObjectOfType<IslandMusicController>()) FindObjectOfType<IslandMusicController>().TriggerMusicStop();
            if(FindObjectOfType<TownMusicController>()) FindObjectOfType<TownMusicController>().TriggerMusicStop();
            if(FindObjectOfType<WavesAmbienceController>()) FindObjectOfType<WavesAmbienceController>().TriggerEventStop();
            if(FindObjectOfType<WavesAmbienceController>()) FindObjectOfType<WavesAmbienceController>().TriggerEventStop();
            if(FindObjectOfType<WavesAmbienceController>()) FindObjectOfType<WavesAmbienceController>().TriggerEventStop();

            DOTween.To(() => mainCanvasGroup.alpha, x => mainCanvasGroup.alpha = x, 1f, fadeAnimationDuration).onComplete += DisplayText;
        }

        /// <summary>
        /// Displays the ending text.
        /// </summary>
        private void DisplayText() {
            mainText.text = string.Empty;
            DOTween.To(() => mainText.text, x => mainText.text = x, tenthDayKey.value, textFadeAnimationDuration).onComplete +=
                () => {
                    StartCoroutine(StartMethodAfterWait(DisplayObjectiveText));
                };
        }

        /// <summary>
        /// Next text bit after initial text.
        /// </summary>
        private void DisplayObjectiveText() {
            mainText.text = string.Empty;
            var coinsText = $"{finalObjectiveKey.value}" +
                            $" {Environment.NewLine} {totalCoinsKey.value} {GameMaster.Instance.PlayerStats.Coins}";
                
            DOTween.To(() => mainText.text, x => mainText.text = x, coinsText, textFadeAnimationDuration).onComplete += () => {
                            StartCoroutine(StartMethodAfterWait(DisplayResultText));
            };
        }

        /// <summary>
        /// Last text bit.
        /// </summary>
        private void DisplayResultText() {
            mainText.text = string.Empty;
            var lastText = GameMaster.Instance.PlayerStats.Coins > finalObjectiveAmount
                               ? successKey.value
                               : failKey.value;

            DOTween.To(() => mainText.text, x => mainText.text = x, lastText, textFadeAnimationDuration).onComplete += () => {
                StartCoroutine(StartMethodAfterWait(DisplayContinueButton));
            };
        }

        /// <summary>
        /// Allows the player to go back to main menu.
        /// </summary>
        private void DisplayContinueButton() {
            SaveSystem.DeleteLoadedFile();
            GameMaster.Instance.SetSaveData(new SaveData());
            GameMaster.Instance.PlayerStats = new PlayerStats(){
            Health = 35, MaxHealth = 35, Stamina = 20, MaxStamina = 20,
            MeleeDamage = 7, RangedDamage = 5, MovementSpeed = 15,
            Level = 1, Experience = 0, TotalExperience = 0,
            Coins = 0, CurrentInventory = new List<InventoryItemEntry>(),
            CurrentUpgradeLevel = 0
            };
            GameMaster.Instance.SaveGame();

            DOTween.To(() => buttonCanvasGroup.alpha, x => buttonCanvasGroup.alpha = x, 1f, fadeAnimationDuration);
            buttonCanvasGroup.interactable = true;
        }

        /// <summary>
        /// Goes back to main menu.
        /// </summary>
        public void ContinueToCredits() {
            SceneManager.LoadSceneAsync(creditsSceneIndex, LoadSceneMode.Additive);
        }

        /// <summary>
        /// Used for delay.
        /// </summary>
        private IEnumerator StartMethodAfterWait(Action returnCall) {
            yield return waitForSeconds;
            returnCall?.Invoke();
        }
    }
}    