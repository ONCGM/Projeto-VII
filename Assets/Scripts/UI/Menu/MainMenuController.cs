using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using Entity.Player;
using FMOD.Studio;
using FMODUnity;
using Game;
using Items;
using Localization;
using UI.Localization;
using UI.Popups;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility;

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
        //[SerializeField] private int gameSceneIndex = 2;
        [SerializeField] private int creditsSceneIndex = 9;
        [SerializeField] private int loadingSceneIndex = 10;
        
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

        [Header("UI Components Reference")]
        [SerializeField] private EventSystem eventSystem;
        [SerializeField] private GameObject selectedButtonMainMenu;
        [SerializeField] private GameObject selectedButtonOptions, selectedButtonLoadGame;
        [Header("Game Tab")]
        [SerializeField] private Slider difficultySlider;
        [SerializeField] private Image ptBrSelectionImage;
        [SerializeField] private Image enSelectionImage;
        [SerializeField] private Image jpSelectionImage;
        [SerializeField] private Image fastGraphicsSelectionImage;
        [SerializeField] private Image prettyGraphicsSelectionImage;
        [SerializeField] private Image windowedSelectionImage;
        [SerializeField] private Image fullscreenSelectionImage;
        [Header("Audio Tab")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private StudioEventEmitter masterEmitter, sfxEmitter;

        #pragma warning restore 0649
        
        #region Unity Events & Setup
        
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

            // Sync Saved Settings
            SyncUiWithSaveFile();
        }

        /// <summary>
        /// Matches the state of the UI to be the same of the save file settings.
        /// </summary>
        private void SyncUiWithSaveFile() {
            var data = GameMaster.Instance.MasterSaveData;
            difficultySlider.value = data.difficulty;
            ptBrSelectionImage.enabled = (data.currentLanguage == LocalizationSystem.Language.Portuguese_Brazil);
            enSelectionImage.enabled = (data.currentLanguage == LocalizationSystem.Language.English);
            jpSelectionImage.enabled = (data.currentLanguage == LocalizationSystem.Language.Japanese);
            fastGraphicsSelectionImage.enabled = !data.graphicsLevel;
            prettyGraphicsSelectionImage.enabled = data.graphicsLevel;
            windowedSelectionImage.enabled = !data.fullscreen;
            fullscreenSelectionImage.enabled = data.fullscreen;
            masterSlider.value = data.audioMasterVolume;
            musicSlider.value = data.audioMusicVolume;
            sfxSlider.value = data.audioSfxVolume;
            
            Invoke(nameof(RemoveGameController), 3f);
            Invoke(nameof(RemoveGameController), 6f);
        }

        /// <summary>
        /// Removes the game controller.
        /// </summary>
        private void RemoveGameController() {
            if(FindObjectOfType<GameMenuController>()) {
                Destroy(FindObjectOfType<GameMenuController>().gameObject);
            }
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
            eventSystem.SetSelectedGameObject(selectedButtonOptions);
            
            DOTween.To(() => loadGameGroup.alpha, x => loadGameGroup.alpha = x, 0f, mainCanvasGroupFadeAnimationSpeed);
            DOTween.To(() => mainMenuGroup.alpha, x=> mainMenuGroup.alpha = x, 0f, mainCanvasGroupFadeAnimationSpeed).onComplete =
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
            eventSystem.SetSelectedGameObject(selectedButtonLoadGame);

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
            eventSystem.SetSelectedGameObject(selectedButtonMainMenu);

            DOTween.To(() => optionsMenuGroup.alpha, x => optionsMenuGroup.alpha = x, 0f, mainCanvasGroupFadeAnimationSpeed);
            DOTween.To(() => loadGameGroup.alpha, x => loadGameGroup.alpha = x, 0f, mainCanvasGroupFadeAnimationSpeed).onComplete =
                () => {
                    DOTween.To(()=> mainMenuGroup.alpha, x=> mainMenuGroup.alpha = x, 1f, mainCanvasGroupFadeAnimationSpeed);
                    loadGameCanvas.enabled = false;
                    optionsMenuCanvas.enabled = false; 
                };
        }

        /// <summary>
        /// Sets the selection on event system.
        /// </summary>
        public void SetEventSystem() {
            if(mainMenuCamera.enabled) eventSystem.SetSelectedGameObject(selectedButtonMainMenu);
            if(optionsMenuCamera.enabled) eventSystem.SetSelectedGameObject(selectedButtonOptions);
            if(loadGameCamera.enabled) eventSystem.SetSelectedGameObject(selectedButtonLoadGame);
        }
        
        #endregion

        #region Button Methods
        
        #region Options Menu

        #region Game Settings
        /// <summary>
        /// Changes the difficulty of the game.
        /// </summary>
        public void ChangeDifficulty(float value) {
            GameMaster.Instance.GameDifficulty = value;
            GameMaster.Instance.MasterSaveData.difficulty = value;
            GameMaster.Instance.SaveGame();
        }

        /// <summary>
        /// Updates the language of the game.
        /// </summary>
        public void ChangeLanguage(int value) {
            LocalizationSystem.CurrentLanguage = (LocalizationSystem.Language) Mathf.Clamp(value, 0, 2);
            GameMaster.Instance.MasterSaveData.currentLanguage = LocalizationSystem.CurrentLanguage;
            GameMaster.Instance.SaveGame();
        }

        /// <summary>
        /// Updates the games graphics settings.
        /// </summary>
        public void ChangeGraphicsQuality(bool value) {
            QualitySettings.SetQualityLevel(value ? 1 : 0, true);
            GraphicsSettings.renderPipelineAsset = value ? highSettingsRenderAsset : lowSettingsRenderAsset;
            GameMaster.Instance.MasterSaveData.graphicsLevel = value;
            GameMaster.Instance.SaveGame();
        }

        /// <summary>
        /// Enables or disables fullscreen.
        /// </summary>
        public void ToggleFullScreen(bool value) {
            Screen.fullScreenMode = value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            GameMaster.Instance.MasterSaveData.fullscreen = value;
            GameMaster.Instance.SaveGame();
        }
        #endregion

        #region Audio Settings
        /// <summary>
        /// Updates the master volume values and stores it.
        /// </summary>
        public void UpdateMasterVolume(float value) {
            masterBus.setVolume(value);
            if(!masterEmitter.IsPlaying() && Time.timeSinceLevelLoad > 3f) masterEmitter.Play();
            GameMaster.Instance.MasterSaveData.audioMasterVolume = value;
            GameMaster.Instance.SaveGame();
        }
        
        /// <summary>
        /// Updates the music volume values and stores it.
        /// </summary>
        public void UpdateMusicVolume(float value) {
            musicBus.setVolume(value);
            GameMaster.Instance.MasterSaveData.audioMusicVolume = value;
            GameMaster.Instance.SaveGame();
        }
        
        /// <summary>
        /// Updates the sfx volume values and stores it.
        /// </summary>
        public void UpdateSFXVolume(float value) {
            sfxBus.setVolume(value);
            if(!sfxEmitter.IsPlaying() && Time.timeSinceLevelLoad > 3f) sfxEmitter.Play();
            GameMaster.Instance.MasterSaveData.audioSfxVolume = value;
            GameMaster.Instance.SaveGame();
        }

        #endregion

        /// <summary>
        /// Loads up the game credits sequence.
        /// </summary>
        public void StartCreditsSequence() {
            GameMaster.Instance.GameSceneWasLoaded = false;
            SceneManager.LoadSceneAsync(creditsSceneIndex, LoadSceneMode.Additive);
        }
        
        #endregion
        
        #region Load Game Menu
        /// <summary>
        /// Loads the game and uses current player save.
        /// </summary>
        public void ContinueGame() {
            GameMaster.Instance.SetSaveData(SaveSystem.LoadGameFile());
            var data = GameMaster.Instance.MasterSaveData;
            GameMaster.Instance.PlayerStats = data.currentPlayerStats;
            GameMaster.Instance.GameDifficulty = data.difficulty;
            GameMaster.Instance.CurrentGameDay = data.gameDay;
            GameMaster.Instance.CurrentTimeOfDay = data.currentTimeOfDay;
            GameMaster.Instance.GameSceneWasLoaded = false;
            SceneManager.LoadSceneAsync(loadingSceneIndex, LoadSceneMode.Additive);
        }
        
        /// <summary>
        /// Loads the game and creates a new save.
        /// </summary>
        public void CreateNewGame() {
            if(newGameCurrentPopup != null) { return; }
            
            loadGameGroup.interactable = false;

            if(GameMaster.Instance.MasterSaveData.brandSpankingNewSave) {
                GameMaster.Instance.GameSceneWasLoaded = false;
                SceneManager.LoadSceneAsync(loadingSceneIndex, LoadSceneMode.Additive);
                return;
            }

            var buttons = new List<CanvasPopupDialog.ButtonSettings>() {
                new CanvasPopupDialog.ButtonSettings(confirmKey.key, CanvasPopupDialog.PopupButtonHighlight.StrongHighlight, 0),
                new CanvasPopupDialog.ButtonSettings(cancelKey.key, CanvasPopupDialog.PopupButtonHighlight.Normal, 1)
            };
            
            var popup = Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();
            popup.SetUpPopup(newGameTitleKey.key, newGameMessageKey.key, buttons, ExecutionState.Normal, i => {
                loadGameGroup.interactable = true;
                if(i >= 1) return;
                loadGameGroup.interactable = false;
                SaveSystem.LoadedData = new SaveData();
                GameMaster.Instance.SetSaveData(new SaveData());
                GameMaster.Instance.MasterSaveData.currentPlayerStats = new PlayerStats() {
                    Health = 35, MaxHealth = 35, Stamina = 20, MaxStamina = 20,
                    MeleeDamage = 7, RangedDamage = 5, MovementSpeed = 15,
                    Level = 1, Experience = 0, TotalExperience = 0,
                    Coins = 0, CurrentInventory = new List<InventoryItemEntry>(),
                    CurrentUpgradeLevel = 0
                };
                GameMaster.Instance.PlayerStats = new PlayerStats() {
                    Health = 35, MaxHealth = 35, Stamina = 20, MaxStamina = 20,
                    MeleeDamage = 7, RangedDamage = 5, MovementSpeed = 15,
                    Level = 1, Experience = 0, TotalExperience = 0,
                    Coins = 0, CurrentInventory = new List<InventoryItemEntry>(),
                    CurrentUpgradeLevel = 0
                };
                GameMaster.Instance.DialogsCleared = new List<bool>();
                GameMaster.Instance.CurrentGameDay = 1;
                GameMaster.Instance.CurrentTimeOfDay = TimeOfDay.Morning;
                GameMaster.Instance.GameSceneWasLoaded = false;
                
                GameMaster.Instance.SaveGame();
                SceneManager.LoadSceneAsync(loadingSceneIndex, LoadSceneMode.Additive);
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