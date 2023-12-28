using System;
using System.Collections;
using System.Globalization;
using DG.Tweening;
using Entity.Player;
using FMODUnity;
using Game;
using Islands;
using Localization;
using TMPro;
using UI.Localization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace UI.Menu {
    /// <summary>
    /// Displays the results of the player exploration.
    /// </summary>
    public class CombatDebriefUi : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Localization Keys")] 
        [SerializeField] private LocalizedString maxLevelKey;

        [SerializeField] private LocalizedString levelKey,
                                                 particleKey,
                                                 upgradeKey,
                                                 nextKey;

        [Header("Prefabs")]
        [SerializeField] private GameObject itemPrefab;
        
        [Header("References")] 
        [SerializeField] private Transform itemListParent;
        [SerializeField] private RectTransform levelBar;
        [SerializeField] private TMP_Text levelText, coinsText, nextUpgradeText;
        [SerializeField] private CanvasGroup levelGroup, coinsGroup, upgradeGroup, buttonsGroup;
        
        [Header("Settings")] 
        [SerializeField, Range(0.1f, 3f)] private float popinAnimationDuration = 0.75f;
        [SerializeField, Range(0.1f, 3f)] private float itemInAnimationDuration = 0.15f;
        [SerializeField, Range(0.1f, 3f)] private float statsAnimationDuration = 0.55f;
        [SerializeField] private GameObject firstSelected;
        
        [Header("Player Settings")]
        private const string upgradeSettingsPath = "Scriptables/Player/Player_Upgrade_Settings";
        [SerializeField] private PlayerUpgradeSettings upgradeSettings;
        
        // Components.
        private CanvasGroup canvasGroup;
        private StudioEventEmitter eventEmitter;
        
        // Yields.
        private WaitForEndOfFrame waitFrame;
        private WaitForSeconds waitForItem;
        private WaitForSeconds waitForStats;
        private WaitForSeconds waitForLetters;
        
        #pragma warning restore 0649

        #region Setup and Navigation. 
        
        // Animates in the canvas and sets up the values.
        private void Awake() {
            eventEmitter = GetComponentInChildren<StudioEventEmitter>();
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            GameMaster.Instance.GameState = ExecutionState.PopupPause;
            upgradeSettings = Resources.Load<PlayerUpgradeSettings>(upgradeSettingsPath);
            waitFrame = new WaitForEndOfFrame();
            waitForItem = new WaitForSeconds(itemInAnimationDuration);
            waitForStats = new WaitForSeconds(statsAnimationDuration);
            waitForLetters = new WaitForSeconds(statsAnimationDuration * 0.1f);
            DOTween.To(()=> canvasGroup.alpha, x=> canvasGroup.alpha = x, 1f, popinAnimationDuration).onComplete =
                () => {
                    StartCoroutine(nameof(FillItemSlots));
                };
        }

        /// <summary>
        /// Closes the canvas and allows the player to keep exploring.
        /// </summary>
        public void KeepExploring() {
            FindObjectOfType<IslandsSceneTransition>().startedTransition = false;
            CloseCanvas();
        }

        /// <summary>
        /// Closes the canvas and starts the ship routine to travel back to town.
        /// </summary>
        public void ReturnToTown() {
            GameMaster.Instance.ShipTravel.StartTravelToTown();
            CloseCanvas(ExecutionState.PopupPause);
        }

        /// <summary>
        /// Animates out the canvas and destroys it.
        /// </summary>
        private void CloseCanvas(ExecutionState state = ExecutionState.Normal) {
            foreach(var inputModule in FindObjectsOfType<InputSystemUIInputModule>()) {
                inputModule.enabled = false;
                inputModule.UpdateModule();
                inputModule.enabled = true;
                inputModule.DeactivateModule();
                inputModule.ActivateModule();
                inputModule.UpdateModule();
            }
            
            GameMaster.Instance.GameState = state;
            DOTween.To(()=> canvasGroup.alpha, x=> canvasGroup.alpha = x, 0f, popinAnimationDuration).onComplete +=
                () => {
                    Destroy(gameObject);
                    foreach(var inputModule in FindObjectsOfType<InputSystemUIInputModule>()) {
                        inputModule.enabled = false;
                        inputModule.UpdateModule();
                        inputModule.enabled = true;
                        inputModule.DeactivateModule();
                        inputModule.ActivateModule();
                        inputModule.UpdateModule();
                    }
                };
        }

        /// <summary>
        /// Unlocks the buttons after animation.
        /// </summary>
        private void UnlockButtons() {
            foreach(var inputModule in FindObjectsOfType<InputSystemUIInputModule>()) {
                inputModule.enabled = false;
                inputModule.UpdateModule();
                inputModule.enabled = true;
                inputModule.DeactivateModule();
                inputModule.ActivateModule();
                inputModule.UpdateModule();
            }
            
            DOTween.To(x => buttonsGroup.alpha = x, 0f, 1f, popinAnimationDuration).onComplete = () => {
                buttonsGroup.interactable = true;
                
                foreach(var eventSystem in FindObjectsOfType<EventSystem>()) {
                    eventSystem.SetSelectedGameObject(firstSelected);
                }

                foreach(var inputModule in FindObjectsOfType<InputSystemUIInputModule>()) {
                    inputModule.enabled = false;
                    inputModule.UpdateModule();
                    inputModule.enabled = true;
                    inputModule.DeactivateModule();
                    inputModule.ActivateModule();
                    inputModule.UpdateModule();
                }
            };
        }
        
        #endregion

        #region UI Data Filling
        
        /// <summary>
        /// Calculates the needed values and fills in
        /// the items in the UI.
        /// </summary>
        private IEnumerator FillItemSlots() {
            var itemEntries = FindObjectOfType<PlayerController>().PlayerIslandInventory.ItemsInInventory;

            foreach(var itemEntry in itemEntries) {
                Instantiate(itemPrefab, itemListParent).GetComponent<SmallItemUI>().SetUpItemUi(itemEntry, itemInAnimationDuration);
                yield return waitForItem;
            }
            
            StartCoroutine(nameof(FillCharacterInfo));
        }

        /// <summary>
        /// Calculates the needed values and fills in
        /// the character info in the UI.
        /// </summary>
        private IEnumerator FillCharacterInfo() {
            var stats = GameMaster.Instance.PlayerStats;
            var oldStats = GameMaster.Instance.PlayerStatsBeforeIsland;
            var player = FindObjectOfType<PlayerController>();

            // Level Bar & Text.
            var animatedLevel = oldStats.Level;
            levelText.text = animatedLevel.ToString();
            var barWidth = levelBar.transform.parent.GetComponent<RectTransform>().rect.width;
            DOTween.To(() => levelGroup.alpha, x => levelGroup.alpha = x, 1f, statsAnimationDuration);
            yield return waitForStats;

            while(animatedLevel < stats.Level) {
                DOTween.To(x => levelBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x), 0f, barWidth, statsAnimationDuration);
                yield return waitForStats;
                animatedLevel++;
                levelText.text = animatedLevel.ToString();
            }
            
            DOTween.To(x => levelBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x), 0f, 
                       Mathf.Lerp(0f,barWidth, Mathf.InverseLerp(0f, upgradeSettings.GetExperienceNeededForLevelUp(stats.Level),
                                                                 stats.Experience)), statsAnimationDuration);
            yield return waitForStats;

            // Coins Text.
            DOTween.To(() => coinsGroup.alpha, x => coinsGroup.alpha = x, 1f, statsAnimationDuration);
            yield return waitForStats;
            
            var coinDifference = stats.Coins - oldStats.Coins;

            DOTween.To(x => coinsText.text = Mathf.RoundToInt(x).ToString(CultureInfo.InvariantCulture), 0, stats.Coins, statsAnimationDuration);
            eventEmitter.Play();
            yield return waitForStats;
            
            DOTween.To(x => coinsText.text = $"{stats.Coins} ({(stats.Coins > 0 ? "+" : string.Empty)}{Mathf.RoundToInt(x)})", 0, coinDifference, statsAnimationDuration);
            eventEmitter.Play();
            yield return waitForStats;
            
            // Upgrade Text.
            DOTween.To(() => upgradeGroup.alpha, x => upgradeGroup.alpha = x, 1f, statsAnimationDuration);
            var levelsRequired = Mathf.RoundToInt((stats.CurrentUpgradeLevel + 1) *  upgradeSettings.upgradeEveryHowManyLevels + 1);
            var upgradeText = "";
            
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch(LocalizationSystem.CurrentLanguage) {
                case LocalizationSystem.Language.Portuguese_Brazil:
                    upgradeText = levelsRequired >= player.MaxLevel ? $"{levelKey.value} {maxLevelKey.value}" :
                                      $"{nextKey.value} {upgradeKey.value} {Environment.NewLine} " +
                                      $"{particleKey.value} {levelKey.value} {levelsRequired}";
                    break;
                case LocalizationSystem.Language.English:
                    upgradeText = levelsRequired >= player.MaxLevel ? $"{maxLevelKey.value} {levelKey.value}" :
                                      $"{nextKey.value} {upgradeKey.value} {Environment.NewLine} " +
                                      $"{particleKey.value} {levelKey.value} {levelsRequired}";
                    break;
                case LocalizationSystem.Language.Japanese:
                    upgradeText = levelsRequired >= player.MaxLevel ? $"{maxLevelKey.value} {levelKey.value}" :
                                     $"{levelKey.value} {levelsRequired} {particleKey.value} {Environment.NewLine} " +
                                     $"{nextKey.value} {upgradeKey.value}";
                    break;
                default:
                    upgradeText = levelsRequired >= player.MaxLevel ? $"{maxLevelKey.value} {levelKey.value}" :
                                      $"{nextKey.value} {upgradeKey.value} {particleKey.value} {levelKey.value} {levelsRequired}";
                    break;
            }

            nextUpgradeText.text = string.Empty;
            
            foreach(var letter in upgradeText.ToCharArray()) {
                nextUpgradeText.text += letter;
                yield return waitForLetters;
            }
            
            yield return waitForStats;
            
            UnlockButtons();
        }

        #endregion
    }
}