using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using DG.Tweening;
using Game;
using UI.Localization;
using UI.Popups;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Menu {
    /// <summary>
    /// Controls the in game menu. Very basic for now.
    /// </summary>
    public class GameMenuController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Prefabs")] 
        [SerializeField] private GameObject popupPrefab;
        
        [Header("Localization")]
        [SerializeField] private LocalizedString gameSavedTitleKey;

        [SerializeField] private LocalizedString gameSavedMessageKey,
                                                 backToMenuTitleKey,
                                                 backToMenuMessageKey,
                                                 quitGameTitleKey,
                                                 quitGameMessageKey,
                                                 gameSaveProgressSavedKey,
                                                 confirmButtonKey,
                                                 cancelButtonKey;
        
        private List<CanvasPopupDialog.ButtonSettings> confirmCancelButtons = new List<CanvasPopupDialog.ButtonSettings>();
        private CanvasGroup mainCanvasGroup;

        [Header("Settings")]
        [SerializeField] private int mainMenuSceneIndex = 1;
        [SerializeField, Range(0.1f, 3f)] private float fadeAnimationDuration = 1f;

        private CanvasPopupDialog popup;
        #pragma warning restore 0649

        // Set up of this class.
        private void Awake() {
            mainCanvasGroup = GetComponent<CanvasGroup>();
            confirmCancelButtons = new List<CanvasPopupDialog.ButtonSettings>() {
                new CanvasPopupDialog.ButtonSettings(confirmButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Normal, 0),
                new CanvasPopupDialog.ButtonSettings(cancelButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Highlight, 1),
            };
        }

        /// <summary>
        /// Saves the game.
        /// </summary>
        public void SaveGame() {
            if(popup != null) return;
            GameMaster.Instance.SaveGame();
            popup = Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();
            var button = new List<CanvasPopupDialog.ButtonSettings> {confirmCancelButtons[0]};

            popup.SetUpPopup(gameSavedTitleKey.key, gameSavedMessageKey.key, button, ExecutionState.PopupPause, i => {});
        }

        /// <summary>
        /// Return to the main menu.
        /// </summary>
        public void ReturnToMainMenu() {
            if(popup != null) return;
            popup = Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();
            
            popup.SetUpPopup(backToMenuTitleKey.key, backToMenuMessageKey.key, gameSaveProgressSavedKey.key, confirmCancelButtons, ExecutionState.PopupPause,
                             i => {
                                 if(i == 0) {
                                     BackToMenu();
                                 }
                             });
        }

        /// <summary>
        /// Goes back to the menu.
        /// </summary>
        private void BackToMenu() {
            DontDestroyOnLoad(gameObject);
            GameMaster.OnReturnToMenu?.Invoke();
            GameMaster.Instance.SaveGame();
            SceneManager.sceneLoaded += OnSceneLoaded;

            for(var i = 0; i < transform.childCount; i++) {
                Destroy(transform.GetChild(i).gameObject);
            }

            var image = GetComponent<Image>();
            image.color = new Color(0f, 0f, 0, 0f);
            image.enabled = true;

            DOTween.To(() => image.color.a, x => image.color = new Color(0f, 0f, 0, x), 1f, fadeAnimationDuration).onComplete =
                () => {
                    SceneManager.LoadSceneAsync(mainMenuSceneIndex, LoadSceneMode.Additive);
                    
                    for(var i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                        if(i == mainMenuSceneIndex) continue;
                        if(SceneManager.GetSceneByBuildIndex(i).isLoaded) SceneManager.UnloadSceneAsync(i);
                    }

                    if(FindObjectOfType<StoreMusicController>()) FindObjectOfType<StoreMusicController>().TriggerMusicStop();
                    if(FindObjectOfType<IslandMusicController>()) FindObjectOfType<IslandMusicController>().TriggerMusicStop();
                    if(FindObjectOfType<TownMusicController>()) FindObjectOfType<TownMusicController>().TriggerMusicStop();
                    if(FindObjectOfType<WavesAmbienceController>()) FindObjectOfType<WavesAmbienceController>().TriggerEventStop();
                    if(FindObjectOfType<WavesAmbienceController>()) FindObjectOfType<WavesAmbienceController>().TriggerEventStop();
                    if(FindObjectOfType<WavesAmbienceController>()) FindObjectOfType<WavesAmbienceController>().TriggerEventStop();
                };
        }
        
        /// <summary>
        /// Quits the game.
        /// </summary>
        public void QuitGame() {
            if(popup != null) return;
            popup = Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();
            
            popup.SetUpPopup(quitGameTitleKey.key, quitGameMessageKey.key, gameSaveProgressSavedKey.key, confirmCancelButtons, ExecutionState.PopupPause,
                             i => {
                                 if(i != 0) return;
                                 GameMaster.Instance.SaveGame();
                                 Invoke(nameof(Quit), fadeAnimationDuration);
                             });
        }

        /// <summary>
        /// Quits the game after confirmation.
        /// </summary>
        private void Quit() {
            Application.Quit(); 
        }
        
        /// <summary>
        /// Removes the canvas. 
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode) {
            for(var i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                if(i == mainMenuSceneIndex) continue;
                if(SceneManager.GetSceneByBuildIndex(i).isLoaded) SceneManager.UnloadSceneAsync(i);
            }
            
            DOTween.To(() => mainCanvasGroup.alpha, x => mainCanvasGroup.alpha = x, 0f, fadeAnimationDuration).onComplete = () => {
                DOTween.Kill(gameObject);
                Destroy(gameObject);
                SceneManager.sceneLoaded -= OnSceneLoaded;
            };
        }
    }
}