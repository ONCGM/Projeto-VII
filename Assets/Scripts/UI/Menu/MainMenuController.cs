using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Game;
using Localization;
using UI.Localization;
using UI.Popups;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace UI.Menu {
    /// <summary>
    /// Controls behaviour for the game's main menu.
    /// </summary>
    public class MainMenuController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Animation Settings")] 
        [SerializeField, Range(0.1f, 5f)] private float mainCanvasGroupFadeAnimationSpeed = 1f;
        
        [Header("Cameras")] 
        [SerializeField] private CinemachineVirtualCamera mainMenuCamera; 
        [SerializeField] private CinemachineVirtualCamera loadGameCamera,
                                                          optionsMenuCamera;

        [Header("Canvasses")] 
        [SerializeField] private Canvas mainMenuCanvas; 
        [SerializeField] private Canvas loadGameCanvas, 
                                        optionsMenuCanvas;
        
        [Header("Canvas Groups")]
        [SerializeField] private CanvasGroup mainMenuGroup;
        [SerializeField] private CanvasGroup loadGameGroup,
                                             optionsMenuGroup;
        
        [Header("Scene Indexes")] 
        [SerializeField] private int gameSceneIndex = 2;
        
        // [Header("Audio Settings")]
        private Bus masterBus;
        private Bus musicBus;
        private Bus sfxBus;
        
        [Header("Graphics Settings")]
        [SerializeField] private RenderPipelineAsset lowSettingsRenderAsset;
        [SerializeField] private RenderPipelineAsset highSettingsRenderAsset;

        [Header("Prefabs")] 
        [SerializeField] private GameObject popupPrefab;

        [Header("Popups")]
        [SerializeField] private LocalizedString confirmKey;
        [SerializeField] private LocalizedString cancelKey;
        
        [Header("New Game Message Popup")]
        [SerializeField] private LocalizedString newGameTitleKey;
        [SerializeField] private LocalizedString newGameMessageKey;
        private CanvasPopupDialog newGameCurrentPopup;
        
        [Header("Quit Message Popup")]
        [SerializeField] private LocalizedString quitTitleKey;
        [SerializeField] private LocalizedString quitMessageKey;
        private CanvasPopupDialog quitCurrentPopup;

        #pragma warning restore 0649
        
        #region Unity Events
        
        // Sets up the class and cameras to be in screen space.
        private void Awake() {
            // Canvasses settings.
            mainMenuCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            loadGameCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            optionsMenuCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            
            // Audio buses.
            masterBus = RuntimeManager.GetBus("bus:/Master");
            musicBus = RuntimeManager.GetBus("bus:/Master/Music");
            sfxBus = RuntimeManager.GetBus("bus:/Master/SFX");

            // TODO: Link save with menu.
        }

        #endregion
        
        #region Navigation

        /// <summary>
        /// Goes to the options menu.
        /// </summary>
        public void ToOptionsMenu() {
            mainMenuCamera.enabled = false;
            loadGameCamera.enabled = false;
            optionsMenuCamera.enabled = true;
            optionsMenuCanvas.enabled = true;

            DOTween.To(() => loadGameGroup.alpha, x => loadGameGroup.alpha = x, 0f, mainCanvasGroupFadeAnimationSpeed);
            DOTween.To(()=> mainMenuGroup.alpha, x=> mainMenuGroup.alpha = x, 0f, mainCanvasGroupFadeAnimationSpeed).onComplete =
                () => {
                    DOTween.To(()=> optionsMenuGroup.alpha, x=> optionsMenuGroup.alpha = x, 1f, mainCanvasGroupFadeAnimationSpeed);
                    mainMenuCanvas.enabled = false;
                    loadGameCanvas.enabled = false; 
                };
        }
        
        /// <summary>
        /// Opens the start game menu.
        /// </summary>
        public void ToLoadGameMenu() {
            mainMenuCamera.enabled = false;
            optionsMenuCamera.enabled = false;
            loadGameCamera.enabled = true;
            loadGameCanvas.enabled = true;

            DOTween.To(() => optionsMenuGroup.alpha, x => optionsMenuGroup.alpha = x, 0f, mainCanvasGroupFadeAnimationSpeed);
            DOTween.To(() => mainMenuGroup.alpha, x => mainMenuGroup.alpha = x, 0f, mainCanvasGroupFadeAnimationSpeed).onComplete =
                () => {
                    DOTween.To(()=> loadGameGroup.alpha, x=> loadGameGroup.alpha = x, 1f, mainCanvasGroupFadeAnimationSpeed);
                    mainMenuCanvas.enabled = false;
                    optionsMenuCanvas.enabled = false; 
                };
        }
        
        /// <summary>
        /// Goes back to the main menu.
        /// </summary>
        public void BackToMenu() {
            optionsMenuCamera.enabled = false;
            loadGameCamera.enabled = false;
            mainMenuCamera.enabled = true;
            mainMenuCanvas.enabled = true;

            DOTween.To(() => optionsMenuGroup.alpha, x => optionsMenuGroup.alpha = x, 0f, mainCanvasGroupFadeAnimationSpeed);
            DOTween.To(() => loadGameGroup.alpha, x => loadGameGroup.alpha = x, 0f, mainCanvasGroupFadeAnimationSpeed).onComplete =
                () => {
                    DOTween.To(()=> mainMenuGroup.alpha, x=> mainMenuGroup.alpha = x, 1f, mainCanvasGroupFadeAnimationSpeed);
                    loadGameCanvas.enabled = false;
                    optionsMenuCanvas.enabled = false; 
                };
        }
        
        #endregion

        #region Button Methods
        
        #region Options Menu

        #region Game Settings
        /// <summary>
        /// Changes the difficulty of the game.
        /// </summary>
        public void ChangeDifficulty(float value) {
            // TODO: Link with game master.
        }

        /// <summary>
        /// Updates the language of the game.
        /// </summary>
        public void ChangeLanguage(int value) {
            LocalizationSystem.CurrentLanguage = (LocalizationSystem.Language) Mathf.Clamp(value, 0, 2);
            
            // TODO: Update game settings with new language.
        }

        /// <summary>
        /// Updates the games graphics settings.
        /// </summary>
        public void ChangeGraphicsQuality(bool value) {
            QualitySettings.SetQualityLevel(value ? 1 : 0, true);
            GraphicsSettings.renderPipelineAsset = value ? highSettingsRenderAsset : lowSettingsRenderAsset;
            
            // TODO: Update game settings.
        }

        /// <summary>
        /// Enables or disables fullscreen.
        /// </summary>
        public void ToggleFullScreen(bool value) {
            Screen.fullScreenMode = value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            
            // TODO: Update game settings.
        }
        #endregion

        #region Audio Settings
        /// <summary>
        /// Updates the master volume values and stores it.
        /// </summary>
        public void UpdateMasterVolume(float value) {
            masterBus.setVolume(value);
            
            // TODO: Update game settings in save.
        }
        
        /// <summary>
        /// Updates the music volume values and stores it.
        /// </summary>
        public void UpdateMusicVolume(float value) {
            musicBus.setVolume(value);
            
            // TODO: Update game settings in save.
        }
        
        /// <summary>
        /// Updates the sfx volume values and stores it.
        /// </summary>
        public void UpdateSFXVolume(float value) {
            sfxBus.setVolume(value);
            
            // TODO: Update game settings in save.
        }

        #endregion

        /// <summary>
        /// Loads up the game credits sequence.
        /// </summary>
        public void StartCreditsSequence() {
            // TODO: Set sequence to play.
        }
        
        #endregion
        
        #region Load Game Menu
        /// <summary>
        /// Loads the game and uses current player save.
        /// </summary>
        public void ContinueGame() {
            // TODO: Load previous game save.
            SceneManager.LoadScene(gameSceneIndex);
        }
        
        /// <summary>
        /// Loads the game and creates a new save.
        /// </summary>
        public void CreateNewGame() {
            if(newGameCurrentPopup != null) { return; }

            loadGameGroup.interactable = false;
            
            var buttons = new List<CanvasPopupDialog.ButtonSettings>() {
                new CanvasPopupDialog.ButtonSettings(confirmKey.key, CanvasPopupDialog.PopupButtonHighlight.StrongHighlight, 0),
                new CanvasPopupDialog.ButtonSettings(cancelKey.key, CanvasPopupDialog.PopupButtonHighlight.Normal, 1)
            };
            
            var popup = Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();
            popup.SetUpPopup(newGameTitleKey.key, newGameMessageKey.key, buttons, ExecutionState.Normal, i => {
                loadGameGroup.interactable = true;
                if(i < 1) {
                    SceneManager.LoadScene(gameSceneIndex);
                    // TODO: Create a new save, delete old.
                } 
            });

            newGameCurrentPopup = popup;
        }
        #endregion
        
        #region Main Menu
        
        /// <summary>
        /// Closes the application.
        /// </summary>
        public void QuitToDesktop() {
            if(quitCurrentPopup != null) { return; }

            mainMenuGroup.interactable = false;
            
            var buttons = new List<CanvasPopupDialog.ButtonSettings>() {
                new CanvasPopupDialog.ButtonSettings(confirmKey.key, CanvasPopupDialog.PopupButtonHighlight.StrongHighlight, 0),
                new CanvasPopupDialog.ButtonSettings(cancelKey.key, CanvasPopupDialog.PopupButtonHighlight.Normal, 1)
            };
            
            var popup = Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();
            popup.SetUpPopup(quitTitleKey.key,quitMessageKey.key, buttons, ExecutionState.Normal, i => {
                mainMenuGroup.interactable = true;
                if(i < 1) Application.Quit(); 
            });

            quitCurrentPopup = popup;
        }
        #endregion
        
        #endregion
    }
}