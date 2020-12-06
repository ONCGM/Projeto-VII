using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Entity.NPC;
using Entity.Player;
using Game;
using Items;
using UI;
using UI.Localization;
using UI.Popups;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

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
        [SerializeField] private List<Transform> itemSpawnPoints = new List<Transform>();
        [SerializeField] public List<Transform> npcSpawnPoints = new List<Transform>();
        [SerializeField] public List<Transform> npcBuyWaypoints = new List<Transform>();
        [SerializeField] public List<Transform> npcPayWaypoints = new List<Transform>();
        [SerializeField] public List<Transform> npcRandomWaypoints = new List<Transform>();
        [SerializeField] private GameObject firstSelected;
        [Header("Settings")]
        [SerializeField, Range(0.1f, 5f)] private float fadeAnimationTime = 1f;
        [SerializeField] private Transform clerkPosition;
        [SerializeField, Range(1, 9)] private int storeInventorySize = 9;
        [SerializeField, Range(0.1f, 1f)] private float priceMarkup = 0.5f;
        [SerializeField, Range(1, 12)] private int maxSpawnedNpcs = 10;
        [SerializeField] private List<NpcStats> npcStats = new List<NpcStats>();
        [SerializeField, Range(1f, 100f)] private float npcSpawnFrequency = 15f; 
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
        [SerializeField] private GameObject tableItemPrefab;
        
        // Components.
        private StoreCanvasTrigger canvasTrigger;
        
        private List<NpcController> spawnedNpcs = new List<NpcController>();
        private WaitForSeconds spawnWait;
        /// <summary>
        /// Is the store open?
        /// </summary>
        public bool StoreOpen { get; private set; }

        // Items
        /// <summary>
        /// The store inventory and the items that the player wants to sell.
        /// </summary>
        public Inventory StoreInventory { get; set; }

        private List<InventoryItemEntry> soldItems = new List<InventoryItemEntry>();

        public List<ItemStore> ItemsToSell { get; set; } = new List<ItemStore>();

        /// <summary>
        /// Creates a duplicate of the player inventory,
        /// so that the player does not looses his items
        /// if the store closes or the game crashes.
        /// Only applies the difference when the store closes.
        /// </summary>
        public Inventory PlayerDuplicateInventory { get; set; }

        public float PriceMarkup {
            get => priceMarkup;
            set => priceMarkup = Mathf.Clamp(value, 0.1f, 1f);
        }

        /// <summary>
        /// Updates the price markup that the player has selected.
        /// </summary>
        public void SetPriceMarkup(float value) => PriceMarkup = value;
        #pragma warning restore 0649

        // Gets references and shit.
        private void Awake() {
            player = FindObjectOfType<PlayerController>();
            canvasTrigger = GetComponentInChildren<StoreCanvasTrigger>();
            spawnWait = new WaitForSeconds(npcSpawnFrequency);
            mainCanvasGroup.alpha = 0f;
            confirmCancelButtons = new List<CanvasPopupDialog.ButtonSettings>() {
                new CanvasPopupDialog.ButtonSettings(confirmButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Normal, 0),
                new CanvasPopupDialog.ButtonSettings(cancelButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Highlight, 1),
            };
            
            StoreInventory = new Inventory(storeInventorySize, new List<InventoryItemEntry>());
            soldItems =  new List<InventoryItemEntry>();
        }

        #region Store Control
        
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
            HideStoreUi(false);
            SpawnItemInTables();
            StartCoroutine(nameof(SpawnNPCs));
            StoreOpen = true;

            foreach(var itemUI in FindObjectsOfType<ItemUI>()) {
                itemUI.LockButtons(StoreOpen);
            }
        }

        /// <summary>
        /// Spawns the selected item from the player in the store.
        /// </summary>
        private void SpawnItemInTables() {
            foreach(var itemStore in ItemsToSell) {
                Destroy(itemStore.gameObject);
            }
            
            ItemsToSell = new List<ItemStore>();
            
            for(var i = 0; i < StoreInventory.ItemsInInventory.Count; i++) {
                var item = Instantiate(tableItemPrefab, itemSpawnPoints[i]).GetComponent<ItemStore>();
                item.SetItemBasedOnSettings(StoreInventory.ItemsInInventory[i].ItemSettings);
                item.itemWaypoint = npcBuyWaypoints[i];
                ItemsToSell.Add(item);
            }
        }

        /// <summary>
        /// Ends the store sequence.
        /// Saves the game and plays the animations for
        /// the npcs and their behaviour leaving the store.
        /// </summary>
        public void CloseStore() {
            foreach(var itemEntry in StoreInventory.ItemsInInventory) {
                player.Inventory.RemoveItemEntry(itemEntry);
            }
            
            StopAllCoroutines();
            
            GameMaster.Instance.SaveGame();
            soldItems = new List<InventoryItemEntry>();
            StoreOpen = false;

            foreach(var itemUI in FindObjectsOfType<ItemUI>()) {
                itemUI.LockButtons(StoreOpen);
            }
            // TODO: Show report.
            // TODO: Unlock Player After Report
        }

        #endregion

        #region NPC Control

        /// <summary>
        /// Spawns the npcs while there are still items in store.
        /// </summary>
        private IEnumerator SpawnNPCs() {
            var maxNpcSpawn = ItemsToSell.Count * 2;
            
            while(ItemsToSell.Count > 0) {
                if(spawnedNpcs.Count >= maxNpcSpawn || spawnedNpcs.Count >= maxSpawnedNpcs) continue;
                
                var npc = GameMaster.Instance.PlayerStats.Level > 10 ? npcStats[Random.Range(0, npcStats.Count)] : npcStats[Random.Range(0, 1)];
                var npcController = Instantiate(npc.npcsVariationsToUse[Random.Range(0, npc.npcsVariationsToUse.Count)],
                                                npcSpawnPoints[Random.Range(0, npcSpawnPoints.Count)].position,
                                                Quaternion.identity).GetComponent<NpcController>();

                npcController.Stats = npc;
                npcController.SetNpcsValuesBasedOnStats();
                spawnedNpcs.Add(npcController);

                yield return spawnWait;
            }
        }

        /// <summary>
        /// Used by the npcs to buy an item.
        /// </summary>
        public ItemStore PickItem(int amountOfMoneyToSpend) {
            var hasItem = ItemsToSell.Any(x => x.Price <= amountOfMoneyToSpend);
            if(!hasItem) return null;
            var item = ItemsToSell.First(x => x.Price * priceMarkup <= amountOfMoneyToSpend);
            ItemsToSell.Remove(item);
            return item;
        }
        
        /// <summary>
        /// Used by the npcs to buy an item.
        /// </summary>
        public int BuyItem(ItemStore item) {
            var itemToRemove = new InventoryItemEntry(item.Settings);
            StoreInventory.RemoveItemEntry(itemToRemove);
            player.Inventory.RemoveItemEntry(itemToRemove);
            player.AddCoins(item.Price);
            return item.Price;
        }

        /// <summary>
        /// Removes a npc from the game and list of npcs.
        /// </summary>
        public void RemoveNpc(NpcController controller) {
            spawnedNpcs.Remove(controller);
            spawnedNpcs.TrimExcess();
            controller.Kill();
        }

        #endregion
        
        #region UI
        /// <summary>
        /// Displays the store UI and stops player from moving.
        /// </summary>
        public void DisplayStoreUi() {
            PlayerDuplicateInventory = new Inventory(player.Inventory.InventorySize, player.Inventory.ItemsInInventory);
            StoreInventory = new Inventory(storeInventorySize, new List<InventoryItemEntry>());
            
            foreach(var eventSystem in FindObjectsOfType<EventSystem>()) {
                eventSystem.SetSelectedGameObject(firstSelected);
            }
            
            UpdateItemDisplays();
            
            DOTween.To(() => mainCanvasGroup.alpha, x => mainCanvasGroup.alpha = x, 1f, fadeAnimationTime)
                   .onComplete = () => {
                player.CanMoveOverride = false;
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
        public void HideStoreUi(bool unlockPlayer = true) {
            DOTween.To(() => mainCanvasGroup.alpha, x => mainCanvasGroup.alpha = x, 0f, fadeAnimationTime);
            player.GetComponent<NavMeshAgent>().enabled = unlockPlayer;
            player.CanMoveOverride = unlockPlayer;
            if(unlockPlayer) canvasTrigger.ResetTrigger();
        }
        
        #endregion
    }
}