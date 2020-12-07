using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using Entity.Player;
using FMODUnity;
using Game;
using Items;
using Store;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Menu {
    /// <summary>
    /// Displays the items that the player has sold.
    /// </summary>
    public class StoreDebriefUi : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Prefabs")]
        [SerializeField] private GameObject itemPrefab;
        
        [Header("References")] 
        [SerializeField] private Transform itemListParent;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private CanvasGroup coinsGroup, buttonsGroup;
        [SerializeField] private StoreController store;
        
        [Header("Settings")] 
        [SerializeField, Range(0.1f, 3f)] private float popInAnimationDuration = 0.75f;
        [SerializeField, Range(0.1f, 3f)] private float itemInAnimationDuration = 0.15f;
        [SerializeField, Range(0.1f, 3f)] private float statsAnimationDuration = 0.55f;
        [SerializeField] private GameObject firstSelected;
        
        
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
            store = FindObjectOfType<StoreController>();
            waitFrame = new WaitForEndOfFrame();
            waitForItem = new WaitForSeconds(itemInAnimationDuration);
            waitForStats = new WaitForSeconds(statsAnimationDuration);
            waitForLetters = new WaitForSeconds(statsAnimationDuration * 0.1f);
            DOTween.To(()=> canvasGroup.alpha, x=> canvasGroup.alpha = x, 1f, popInAnimationDuration).onComplete =
                () => {
                    StartCoroutine(nameof(FillItemSlots));
                };
        }

        /// <summary>
        /// Closes the canvas and allows the player to keep exploring.
        /// </summary>
        public void CloseReport() {
            CloseCanvas();
            store.CloseStore();
        }

        /// <summary>
        /// Animates out the canvas and destroys it.
        /// </summary>
        private void CloseCanvas(ExecutionState state = ExecutionState.Normal) {
            GameMaster.Instance.GameState = state;
            DOTween.To(()=> canvasGroup.alpha, x=> canvasGroup.alpha = x, 0f, popInAnimationDuration).onComplete +=
                () => {
                    Destroy(gameObject);
                };
        }

        /// <summary>
        /// Unlocks the buttons after animation.
        /// </summary>
        private void UnlockButtons() {
            DOTween.To(x => buttonsGroup.alpha = x, 0f, 1f, popInAnimationDuration).onComplete = () => {
                buttonsGroup.interactable = true;
                
                foreach(var eventSystem in FindObjectsOfType<EventSystem>()) {
                    eventSystem.SetSelectedGameObject(firstSelected);
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
            var itemEntries = store.SoldItems;

            foreach(var itemEntry in itemEntries) {
                Instantiate(itemPrefab, itemListParent).GetComponent<SmallItemUI>().SetUpItemUi(new InventoryItemEntry(itemEntry), itemInAnimationDuration);
                yield return waitForItem;
            }
            
            StartCoroutine(nameof(FillSaleInfo));
        }

        /// <summary>
        /// Calculates the needed values and fills in
        /// the character info in the UI.
        /// </summary>
        private IEnumerator FillSaleInfo() {
            var player = FindObjectOfType<PlayerController>();
            
            // Coins Text.
            DOTween.To(() => coinsGroup.alpha, x => coinsGroup.alpha = x, 1f, statsAnimationDuration);
            yield return waitForStats;
            
            var coinDifference = store.ProfitFromLastSale;

            DOTween.To(x => coinsText.text = Mathf.RoundToInt(x).ToString(CultureInfo.InvariantCulture), 0, player.Coins, statsAnimationDuration);
            eventEmitter.Play();
            yield return waitForStats;
            
            DOTween.To(x => coinsText.text = $"{player.Coins} (+{Mathf.RoundToInt(x)})", 0, coinDifference, statsAnimationDuration);
            eventEmitter.Play();
            yield return waitForStats;

            // Buttons.
            UnlockButtons();
        }

        #endregion
    }
}