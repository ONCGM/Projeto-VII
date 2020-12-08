using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DG.Tweening;
using Entity.Player;
using Game;
using Items;
using TMPro;
using UI;
using UI.Localization;
using UI.Popups;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace Entity.NPC {
    /// <summary>
    /// Controls the merchant npc store.
    /// </summary>
    public class MerchantNPC : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")]
        [SerializeField] private bool isOpen;
        [SerializeField, Range(1, 9)] private int howManyItemsToGenerate;
        [SerializeField] private List<ItemSettings> itemsToSell = new List<ItemSettings>();
        [SerializeField, Range(0.1f, 5f)] private float fadeAnimationTime = 1f;
        [SerializeField, Range(1, 9)] private int merchantInventorySize = 9;
        [SerializeField, Range(0.1f, 2f)] private float priceMarkup = 1f;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private string playerStatsTag = "Player Components";

        [Header("References")] 
        [SerializeField] private Transform npcInventoryParent;
        [SerializeField] private Transform playerInventoryParent;
        
        [Header("Components")]
        [SerializeField] private CanvasGroup mainCanvasGroup;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private GameObject firstSelected;
        [SerializeField] private PlayerStatsUI mainStatsUi, merchantStatsUi;
        
        [Header("Prefabs")] 
        [SerializeField] private GameObject popupPrefab;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private GameObject itemPrefabWithButton;
        
        [Header("Localization")] 
        [SerializeField] private LocalizedString cantBuyWithoutItemsTitleKey;

        [SerializeField] private LocalizedString cantBuyWithoutItemsMessageKey,
                                                 cantBuyWithoutCoinsTitleKey,
                                                 cantBuyWithoutCoinsMessageKey,
                                                 buyItemsTitleKey, buyItemsMessageKey,
                                                 confirmButtonKey, cancelButtonKey;

        // Variables and Stuff.
        private PlayerController player;
        
        private Inventory npcInventory;

        private List<ItemSettings> selectedItems = new List<ItemSettings>();

        private List<CanvasPopupDialog.ButtonSettings> confirmCancelButtons = new List<CanvasPopupDialog.ButtonSettings>();
        #pragma warning restore 0649
        
        // Sets up stuff.
        private void Awake() {
            // Self-destruct if not merchant island.
            if(GameMaster.Instance.CurrentIslandType != IslandType.MerchantIsland) {
                Destroy(gameObject);
                return;
            }

            confirmCancelButtons = new List<CanvasPopupDialog.ButtonSettings>() {
                new CanvasPopupDialog.ButtonSettings(confirmButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Normal, 0),
                new CanvasPopupDialog.ButtonSettings(cancelButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Highlight, 1),
            };
            
            var itemsToCreate = Mathf.Lerp(1, howManyItemsToGenerate, Mathf.InverseLerp(1, 30, GameMaster.Instance.PlayerStats.Level));
            var usableItems = itemsToSell
                              .Where(x => GameMaster.Instance.PlayerStats.Level >= x.minimumPlayerLevelToSpawn)
                              .ToList();
            
            npcInventory = new Inventory(merchantInventorySize, new List<InventoryItemEntry>());
            
            for(var i = 0; i < itemsToCreate; i++) {
                if(npcInventory.ItemsInInventory.Count < npcInventory.InventorySize - 1) npcInventory.
                    AddItemEntry(new InventoryItemEntry(usableItems[Random.Range(0, usableItems.Count)]), 1, false);
            }
        }

        // Finds the player.
        private void Start() {
            player = FindObjectOfType<PlayerController>();
            foreach(var find in GameObject.FindGameObjectsWithTag(playerStatsTag)) {
                if(!find.GetComponent<PlayerStatsUI>()) continue;
                mainStatsUi = find.GetComponent<PlayerStatsUI>();
            }
        }

        #region Store Controls
        /// <summary>
        /// Checks if the sale can be made.
        /// </summary>
        public void TriggerSaleStore() {
            if(player.Coins < selectedItems.Sum(x => x.initialValue)) {
                var noMoneyPopup = Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();
                noMoneyPopup.SetUpPopup(cantBuyWithoutCoinsTitleKey.key, cantBuyWithoutCoinsMessageKey.key, 
                                      new List<CanvasPopupDialog.ButtonSettings>{
                                          new CanvasPopupDialog.ButtonSettings(confirmButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Normal, 0)
                                      }, ExecutionState.PopupPause, i => { CancelPurchase(); HideUi(); });
                return;
            }

            if(selectedItems.Count < 1) {
                var itemPopup = Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();
                itemPopup.SetUpPopup(cantBuyWithoutItemsTitleKey.key, cantBuyWithoutItemsMessageKey.key, 
                                     new List<CanvasPopupDialog.ButtonSettings>{
                                         new CanvasPopupDialog.ButtonSettings(confirmButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Normal, 0)
                                     }, ExecutionState.PopupPause, i => { HideUi(); });
                return;
            }

            var popup = Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();
            popup.SetUpPopup(buyItemsTitleKey.key,
                             buyItemsMessageKey.key,
                             confirmCancelButtons, ExecutionState.PopupPause, i => {
                                 if(i == 0) {
                                     BuyItems();
                                 } else {
                                     CancelPurchase();
                                 }
                             });
        }
        
        
        /// <summary>
        /// Ends the store sequence.
        /// Saves the game and plays the animations for
        /// the npcs and their behaviour leaving the store.
        /// </summary>
        public void BuyItems() {
            player.AddCoins(-selectedItems.Sum(x => x.initialValue));
            
            StopAllCoroutines();
            
            GameMaster.Instance.SaveGame();
            isOpen = false;

            HideUi();
        }

        /// <summary>
        /// Cancels the purchase.
        /// </summary>
        public void CancelPurchase() {
            foreach(var selectedItem in selectedItems) {
                AddToStoreInventory(new InventoryItemEntry(selectedItem));
            }
            
            selectedItems = new List<ItemSettings>();
        }
        
        /// <summary>
        /// Displays the store UI and stops player from moving.
        /// </summary>
        public void DisplayMerchantUi() {
            isOpen = true;
            
            selectedItems = new List<ItemSettings>();
            
            foreach(var itemUI in FindObjectsOfType<ItemUI>()) {
                itemUI.LockButtons(isOpen);
            }

            foreach(var eventSystem in FindObjectsOfType<EventSystem>()) {
                eventSystem.SetSelectedGameObject(firstSelected);
            }
            
            UpdateItemDisplays();
            
            mainStatsUi.ShowHideCanvas(false);
            merchantStatsUi.ShowHideCanvas(true);
            
            DOTween.To(() => mainCanvasGroup.alpha, x => mainCanvasGroup.alpha = x, 1f, fadeAnimationTime)
                   .onComplete = () => {
                player.CanMoveOverride = false;
                player.GetComponent<NavMeshAgent>().enabled = false;
            };
        }
        
        /// <summary>
        /// Hides the merchant UI and allows the player to move again.
        /// </summary>
        public void HideUi() {
            foreach(var itemUI in FindObjectsOfType<ItemUI>()) {
                itemUI.LockButtons(isOpen);
            }

            mainStatsUi.ShowHideCanvas(true);
            merchantStatsUi.ShowHideCanvas(false);
            
            DOTween.To(() => mainCanvasGroup.alpha, x => mainCanvasGroup.alpha = x, 0f, fadeAnimationTime).onComplete +=
                () => {
                    player.GetComponent<NavMeshAgent>().enabled = true;
                    player.CanMoveOverride = true;
                    isOpen = false;
                };
        }
        
        /// <summary>
        /// Updates the visuals on the item displays.
        /// </summary>
        private void UpdateItemDisplays() {
            for(var i = 0; i < playerInventoryParent.childCount; i++) {
                Destroy(playerInventoryParent.GetChild(i).gameObject);
            }
            
            for(var i = 0; i < npcInventoryParent.childCount; i++) {
                Destroy(npcInventoryParent.GetChild(i).gameObject);
            }
            
            foreach(var itemEntry in player.Inventory.ItemsInInventory) {
                var item = Instantiate(itemPrefab, playerInventoryParent).GetComponent<SmallItemUI>();
                item.SetUpItemUi(itemEntry, 0f);
            }

            foreach(var itemEntry in npcInventory.ItemsInInventory) {
                var item = Instantiate(itemPrefabWithButton, npcInventoryParent).GetComponent<SmallItemUIButton>();
                item.SetUpItemUi(itemEntry, 0f, () => RemoveFromStoreInventory(itemEntry));
            }
            
            var totalPrice = selectedItems.Sum(x => x.initialValue * priceMarkup);

            DOTween.To(x => priceText.text = Mathf.RoundToInt(x).ToString(CultureInfo.InvariantCulture), int.Parse(priceText.text), totalPrice, Time.deltaTime * 2f);
        }

        /// <summary>
        /// Adds an item to the store inventory and removes it from the player's.
        /// </summary>
        private void AddToStoreInventory(InventoryItemEntry itemEntry) {
            if(npcInventory.ItemsInInventory.Count >= merchantInventorySize) return;
            npcInventory.AddItemEntry(itemEntry, 1, false);
            player.Inventory.RemoveItemEntry(itemEntry);
            UpdateItemDisplays();
        }
        
        /// <summary>
        /// Removes an item from the store inventory and adds it to the player's.
        /// </summary>
        private void RemoveFromStoreInventory(InventoryItemEntry itemEntry) {
            if(player.Inventory.ItemsInInventory.Count >= player.Inventory.InventorySize) return;
            player.Inventory.AddItemEntry(itemEntry, 1, false);
            npcInventory.RemoveItemEntry(itemEntry);
            selectedItems.Add(itemEntry.ItemSettings);
            UpdateItemDisplays();
        }

        #endregion
        
        // Collisions and ui open.
        private void OnTriggerEnter(Collider other) {
            if(!other.CompareTag(playerTag) || isOpen) return;
            DisplayMerchantUi();
        }
    }
}