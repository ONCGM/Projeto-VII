using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Entity.Player;
using Game;
using Items;
using UI;
using UI.Localization;
using UI.Popups;
using UnityEngine;
using UnityEngine.AI;

namespace Store {
    /// <summary>
    /// Controls the store behaviour and shit.
    /// </summary>
    public class StoreController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("References")] 
        [SerializeField] private PlayerController player;
        [SerializeField] private CanvasGroup mainCanvasGroup;
        [SerializeField] private Transform playerInventoryParent, storeInventoryParent;
        [Header("Settings")]
        [SerializeField, Range(0.1f, 5f)] private float fadeAnimationTime = 1f;
        [SerializeField] private Transform clerkPosition;
        [SerializeField, Range(1, 9)] private int storeInventorySize = 9;
        [SerializeField, Range(0.1f, 1f)] private float priceMarkup = 0.5f;
        [Header("Localization")] 
        [SerializeField] private LocalizedString openStoreTitleKey;

        [SerializeField] private LocalizedString openStoreMessageKey,
                                                 timeAdvanceMessageKey,
                                                 confirmButtonKey,
                                                 cancelButtonKey,
                                                 cantOpenAtNightTitleKey, 
                                                 cantOpenAtNightMessageKey,
                                                 cantOpenWithoutItemsTitleKey, 
                                                 cantOpenWithoutItemsMessageKey;
        
        // Basic Buttons.
        private List<CanvasPopupDialog.ButtonSettings> confirmCancelButtons =
            new List<CanvasPopupDialog.ButtonSettings>();
        
        [Header("Prefabs")] 
        [SerializeField] private GameObject popupPrefab;
        [SerializeField] private GameObject itemPrefab;
        
        // Components.
        private StoreCanvasTrigger canvasTrigger;

        // Items
        /// <summary>
        /// The store inventory and the items that the player wants to sell.
        /// </summary>
        public Inventory StoreInventory { get; set; }
        /// <summary>
        /// Creates a duplicate of the player inventory,
        /// so that the player does not looses its items
        /// if the store closes or the game crashes.
        /// Only applies the difference when the store closes.
        /// </summary>
        public Inventory PlayerDuplicateInventory { get; set; }

        public float PriceMarkup {
            get => priceMarkup;
            set => priceMarkup = Mathf.Clamp(value, 0.1f, 1f);
        }
        #pragma warning restore 0649

        // Gets references and shit.
        private void Awake() {
            player = FindObjectOfType<PlayerController>();
            canvasTrigger = GetComponentInChildren<StoreCanvasTrigger>();
            mainCanvasGroup.alpha = 0f;
            confirmCancelButtons = new List<CanvasPopupDialog.ButtonSettings>() {
                new CanvasPopupDialog.ButtonSettings(confirmButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Normal, 0),
                new CanvasPopupDialog.ButtonSettings(cancelButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Highlight, 1),
            };
            
            StoreInventory = new Inventory(storeInventorySize, new List<InventoryItemEntry>());
        }
        
        /// <summary>
        /// Checks if the store can be opened.
        /// </summary>
        public void TriggerOpenStore() {
            if(GameMaster.Instance.CurrentTimeOfDay == TimeOfDay.Night) {
                var nightPopup = Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();
                nightPopup.SetUpPopup(cantOpenAtNightTitleKey.key, cantOpenAtNightMessageKey.key, 
                                 new List<CanvasPopupDialog.ButtonSettings>{
                                     new CanvasPopupDialog.ButtonSettings(confirmButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Normal, 0)
                                 }, ExecutionState.PopupPause, i => { HideStoreUi(); });
                return;
            }

            if(StoreInventory.ItemsInInventory.Count < 1) {
                var itemPopup = Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();
                itemPopup.SetUpPopup(cantOpenWithoutItemsTitleKey.key, cantOpenWithoutItemsMessageKey.key, 
                                      new List<CanvasPopupDialog.ButtonSettings>{
                                          new CanvasPopupDialog.ButtonSettings(confirmButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Normal, 0)
                                      }, ExecutionState.PopupPause, i => { HideStoreUi(); });
                return;
            }

            var popup = Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();
            popup.SetUpPopup(openStoreTitleKey.key,
                             openStoreMessageKey.key,
                             timeAdvanceMessageKey.key,
                             confirmCancelButtons, ExecutionState.PopupPause, i => {
                                 if(i == 0) {
                                     OpenStore();
                                 } 
                             });
        }

        /// <summary>
        /// Starts the store sequence.
        /// Skips a time period and plays the animations for
        /// the npcs and their behaviour entering in the store.
        /// </summary>
        private void OpenStore() {
            GameMaster.Instance.AdvanceOneTimePeriod();
            GameMaster.Instance.SaveGame();
            HideStoreUi();
        }
        
        /// <summary>
        /// Ends the store sequence.
        /// Saves the game and plays the animations for
        /// the npcs and their behaviour leaving the store.
        /// </summary>
        public void CloseStore() {
            
        }

        #region UI
        /// <summary>
        /// Displays the store UI and stops player from moving.
        /// </summary>
        public void DisplayStoreUi() {
            PlayerDuplicateInventory = new Inventory(player.Inventory.InventorySize, player.Inventory.ItemsInInventory);
            StoreInventory = new Inventory(storeInventorySize, new List<InventoryItemEntry>());
            UpdateItemDisplays();
            
            DOTween.To(() => mainCanvasGroup.alpha, x => mainCanvasGroup.alpha = x, 1f, fadeAnimationTime)
                   .onComplete = () => {
                player.CanMove = false;
                player.GetComponent<NavMeshAgent>().enabled = false;
                player.transform.position = clerkPosition.position;
                // ReSharper disable once Unity.InefficientPropertyAccess
                player.transform.rotation = clerkPosition.rotation;
            };
        }

        /// <summary>
        /// Updates the visuals on the item displays.
        /// </summary>
        private void UpdateItemDisplays() {
            for(var i = 0; i < playerInventoryParent.childCount; i++) {
                Destroy(playerInventoryParent.GetChild(i).gameObject);
            }
            
            for(var i = 0; i < storeInventoryParent.childCount; i++) {
                Destroy(storeInventoryParent.GetChild(i).gameObject);
            }
            
            foreach(var itemEntry in PlayerDuplicateInventory.ItemsInInventory) {
                var item = Instantiate(itemPrefab, playerInventoryParent).GetComponent<SmallItemUIButton>();
                item.SetUpItemUi(itemEntry, 0f, () => AddToStoreInventory(itemEntry));
            }

            foreach(var itemEntry in StoreInventory.ItemsInInventory) {
                var item = Instantiate(itemPrefab, storeInventoryParent).GetComponent<SmallItemUIButton>();
                item.SetUpItemUi(itemEntry, 0f, () => RemoveFromStoreInventory(itemEntry));
            }
        }

        private void AddToStoreInventory(InventoryItemEntry itemEntry) {
            if(StoreInventory.ItemsInInventory.Count >= storeInventorySize) return;
            StoreInventory.AddItemEntry(itemEntry);
            PlayerDuplicateInventory.RemoveItemEntry(itemEntry);
            UpdateItemDisplays();
        }
        
        private void RemoveFromStoreInventory(InventoryItemEntry itemEntry) {
            if(PlayerDuplicateInventory.ItemsInInventory.Count >= PlayerDuplicateInventory.InventorySize) return;
            PlayerDuplicateInventory.AddItemEntry(itemEntry);
            StoreInventory.RemoveItemEntry(itemEntry);
            UpdateItemDisplays();
        }
        
        /// <summary>
        /// Hides the store UI and allows the player to move again.
        /// </summary>
        public void HideStoreUi() {
            DOTween.To(() => mainCanvasGroup.alpha, x => mainCanvasGroup.alpha = x, 0f, fadeAnimationTime);
            player.GetComponent<NavMeshAgent>().enabled = true;
            player.CanMove = true;
            canvasTrigger.ResetTrigger();
        }
        
        #endregion
    }
}