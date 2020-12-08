using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Entity.Player;
using Game;
using Localization;
using TMPro;
using UI.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI {
    /// <summary>
    /// Allows the player to restart the game.
    /// </summary>
    public class DeathCanvasUI : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")]
        [SerializeField, Range(0.5f, 3f)] private float fadeAnimationDuration = 2f;
        [SerializeField] private CanvasGroup mainCanvasGroup;
        [SerializeField] private CanvasGroup textCanvasGroup;
        [SerializeField] private CanvasGroup buttonsCanvasGroup;
        [SerializeField] private int mainMenuSceneIndex = 1;
        [SerializeField] private LocalizedString youWereKilledByKey;
        [SerializeField] private LocalizedString deathCountKey;
        [SerializeField] private LocalizedString deathCountEndKey;

        [Header("Components")] 
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text deadText;
        private bool startTransition;

        private PlayerController player;
        #pragma warning restore 0649

        // Sets up the class.
        private void Awake() {
            player = FindObjectOfType<PlayerController>();
            if(LocalizationSystem.CurrentLanguage == LocalizationSystem.Language.Japanese) {
                deadText.text = $"{player.LastEnemyToHitPlayer.enemyType.ToString()} {youWereKilledByKey.value} " +
                                $"{Environment.NewLine} {deathCountKey.value} {GameMaster.Instance.MasterSaveData.playerDeathCount} {deathCountEndKey.value}";
            } else {
                deadText.text = $"{youWereKilledByKey.value} {player.LastEnemyToHitPlayer.enemyType.ToString()}. " +
                                $"{Environment.NewLine} {deathCountKey.value} {GameMaster.Instance.MasterSaveData.playerDeathCount} {deathCountEndKey.value}";
            }

            mainCanvasGroup.alpha = 0f;
            textCanvasGroup.alpha = 0f;
            buttonsCanvasGroup.alpha = 0f;
            buttonsCanvasGroup.interactable = false;
            DOTween.To(() => mainCanvasGroup.alpha, x => mainCanvasGroup.alpha = x, 1f, fadeAnimationDuration).onComplete =
                () => {
                    DOTween.To(() => textCanvasGroup.alpha, x => textCanvasGroup.alpha = x, 1f, fadeAnimationDuration)
                           .onComplete =
                        () => {
                            DOTween.To(() => buttonsCanvasGroup.alpha, x => buttonsCanvasGroup.alpha = x, 1f,
                                       fadeAnimationDuration).onComplete = () => {
                                buttonsCanvasGroup.interactable = true;};
                        };

                };
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// Continues the game and sets player back at port.
        /// </summary>
        public void Continue() {
            if(startTransition) return;
            if(player == null) player = FindObjectOfType<PlayerController>();
            player.ResetPlayer();
            DOTween.To(() => mainCanvasGroup.alpha, x => mainCanvasGroup.alpha = x, 0f, fadeAnimationDuration).onComplete += () => Destroy(gameObject);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            startTransition = true;
        }

        /// <summary>
        /// Goes back to menu.
        /// </summary>
        public void BackToMenu() {
            if(startTransition) return;
            DontDestroyOnLoad(gameObject);
            GameMaster.OnReturnToMenu?.Invoke();
            GameMaster.Instance.SaveGame();
            
            DOTween.To(() => mainCanvasGroup.alpha, x => mainCanvasGroup.alpha = x, 1f, fadeAnimationDuration).onComplete =
                () => {
                    SceneManager.LoadSceneAsync(mainMenuSceneIndex, LoadSceneMode.Additive);
                    
                    for(var i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                        if(i == mainMenuSceneIndex) continue;
                        if(SceneManager.GetSceneByBuildIndex(i).isLoaded) SceneManager.UnloadSceneAsync(i);
                    }
                };
            
            startTransition = true;
        }

        /// <summary>
        /// Called to remove overlay when going to main menu.
        /// </summary>
        private void OnSceneLoaded(Scene arg0, LoadSceneMode loadSceneMode) {
            if(arg0.buildIndex != mainMenuSceneIndex) return;
            DOTween.To(() => mainCanvasGroup.alpha, x => mainCanvasGroup.alpha = x, 0.98f, fadeAnimationDuration)
                   .onComplete += () => {
                
                for(var i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                    if(i == mainMenuSceneIndex) continue;
                    if(SceneManager.GetSceneByBuildIndex(i).isLoaded) SceneManager.UnloadSceneAsync(i);
                }
                
                DOTween.To(() => mainCanvasGroup.alpha, x => mainCanvasGroup.alpha = x, 0f,
                           fadeAnimationDuration)
                       .onComplete += () => { Destroy(gameObject); };
            };
        }
    }
}