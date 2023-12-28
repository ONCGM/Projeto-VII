using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DG.Tweening;
using Entity.NPC;
using Entity.Player;
using FMODUnity;
using Game;
using Items;
using TMPro;
using UI;
using UI.Localization;
using UI.Popups;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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
        [SerializeField] private Transform playerInventoryParent, 
                                           storeInventoryParent, 
                                           coinsSpawnPoint;
        [SerializeField] public Transform itemPurchasePosition;
        [SerializeField] private List<Transform> itemSpawnPoints = new List<Transform>();
        [SerializeField] public List<Transform> npcSpawnPoints = new List<Transform>();
        [SerializeField] public List<Transform> npcBuyWaypoints = new List<Transform>();
        [SerializeField] public List<Transform> npcPayWaypoints = new List<Transform>();
        [SerializeField] public List<Transform> npcRandomWaypoints = new List<Transform>();
        [SerializeField] private GameObject firstSelected;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private Slider priceSlider;
        [SerializeField] private CloseStoreUI closeStoreUI;
        [Header("Settings")]
        [SerializeField, Range(0.1f, 5f)] private float fadeAnimationTime = 1f;
        [SerializeField] private Transform clerkPosition;
        [SerializeField, Range(0.1f, 5f)] private float coinSpawnVariation = 2f;
        [SerializeField, Range(1, 9)] private int storeInventorySize = 9;
        [SerializeField, Range(0.1f, 2f)] private float priceMarkup = 1f;
        [SerializeField, Range(1, 12)] private int maxSpawnedNpcs = 10;
        [SerializeField] private List<NpcStats> npcStats = new List<NpcStats>();
        [SerializeField, Range(1f, 100f)] private float npcSpawnFrequency = 15f;
        [Header("Audio Settings")]
        [SerializeField, EventRef] private string storeOpenEvent;
        [SerializeField, EventRef] private string coinsPurchaseEvent;
        [SerializeField] private StudioEventEmitter eventEmitter;
        [SerializeField] private StudioEventEmitter purchaseEventEmitter;
        
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
        [SerializeField] private GameObject reportPrefab;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private GameObject tableItemPrefab;

        [SerializeField] private GameObject coinPrefab,
                                            coinStackPrefab,
                                            coinMountainPrefab;
            
        
        // Components.
        private StoreCanvasTrigger canvasTrigger;

        public List<NpcController> SpawnedNpcs { get; private set; }  = new List<NpcController>();
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
        public List<ItemStore> ItemsToSell { get; set; } = new List<ItemStore>();

        public List<ItemSettings> SoldItems { get; private set; } = new List<ItemSettings>();

        /// <summary>
        /// How many coins made last round.
        /// </summary>
        public int ProfitFromLastSale { get; private set; }

        /// <summary>
        /// Creates a duplicate of the player inventory,
        /// so that the player does not looses his items
        /// if the store closes or the game crashes.
        /// Only applies the difference when the store closes.
        /// </summary>
        public Inventory PlayerDuplicateInventory { get; set; }

        public float PriceMarkup {
            get => priceMarkup;
            set => priceMarkup = Mathf.Clamp(value, 0.1f, 2f);
        }
        #pragma warning restore 0649

        // Gets references and shit.
        private void Awake() {
            eventEmitter = GetComponentInChildren<StudioEventEmitter>();
            player = FindObjectOfType<PlayerController>();
            canvasTrigger = GetComponentInChildren<StoreCanvasTrigger>();
            spawnWait = new WaitForSeconds(npcSpawnFrequency);
            mainCanvasGroup.alpha = 0f;
            confirmCancelButtons = new List<CanvasPopupDialog.ButtonSettings>() {
                new CanvasPopupDialog.ButtonSettings(confirmButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Normal, 0),
                new CanvasPopupDialog.ButtonSettings(cancelButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Highlight, 1),
            };
            
            StoreInventory = new Inventory(storeInventorySize, new List<InventoryItemEntry>());
            SoldItems = new List<ItemSettings>();
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
                                 if(i != 0) return;
                                 if(FindObjectOfType<FloorController>().currentFloor == 0) OpenStore();
                             });
        }

        /// <summary>
        /// Starts the store sequence.
        /// Skips a time period and plays the animations for
        /// the npcs and their behaviour entering in the store.
        /// </summary>
        private void OpenStore() {
            GameMaster.Instance.AdvanceOneTimePeriod();
            eventEmitter.Play();
            HideStoreUi(false);
            SpawnItemInTables();
            StartCoroutine(nameof(SpawnNPCs));
            StoreOpen = true;
            ProfitFromLastSale = 0;
            SoldItems = new List<ItemSettings>();
            RemoveCoinsDisplay();
            closeStoreUI.DisplayUi();

            foreach(var itemUI in FindObjectsOfType<ItemUI>()) {
                itemUI.LockButtons(StoreOpen);
            }
            
            InvokeRepeating(nameof(CheckForItemsToSell), 3f, 1f);
        }

        /// <summary>
        /// Checks to see if all items have been sold.
        /// </summary>
        private void CheckForItemsToSell() {
            if(ItemsToSell.Count > 0 || SpawnedNpcs.Count > 0) return;
            if(closeStoreUI.IsOpen) closeStoreUI.HideUi();
            Instantiate(reportPrefab);
            CancelInvoke(nameof(CheckForItemsToSell));
        }

        /// <summary>
        /// Spawns the selected item from the player in the store.
        /// </summary>
        private void SpawnItemInTables() {
            foreach(var itemStore in ItemsToSell.Where(itemStore => itemStore != null)) {
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
        /// Updates the price markup.
        /// </summary>
        public void SetPriceMarkup(float value) {
            PriceMarkup = value;

            var totalPrice = StoreInventory.ItemsInInventory.Sum(x => x.ItemSettings.initialValue * priceMarkup);

            DOTween.To(x => priceText.text = Mathf.RoundToInt(x).ToString(CultureInfo.InvariantCulture), int.Parse(priceText.text), totalPrice, Time.deltaTime);
        }

        /// <summary>
        /// Ends the store sequence.
        /// Saves the game and plays the animations for
        /// the npcs and their behaviour leaving the store.
        /// </summary>
        public void CloseStore() {
            StopAllCoroutines();
            
            GameMaster.Instance.SaveGame();
            StoreOpen = false;

            foreach(var itemUI in FindObjectsOfType<ItemUI>()) {
                itemUI.LockButtons(StoreOpen);
            }
            
            StoreInventory = new Inventory(storeInventorySize, new List<InventoryItemEntry>());
            
            player.CanMoveOverride = true;
            player.GetComponent<NavMeshAgent>().enabled = true;
            canvasTrigger.ResetTrigger();
        }

        /// <summary>
        /// Closes the store early. By player choice.
        /// </summary>
        public void CloseEarly() {
            Instantiate(reportPrefab);
            CancelInvoke(nameof(CheckForItemsToSell));
            foreach(var npc in FindObjectsOfType<NpcController>()) {
                npc.LeaveStore();
            }

            foreach(var spawnPoint in itemSpawnPoints) {
                for(var i = 0; i < spawnPoint.childCount; i++) {
                       Destroy(spawnPoint.GetChild(i).gameObject); 
                }
            }
        }

        /// <summary>
        /// Adds more coins to the stack.
        /// </summary>
        private void AddCoinsToDisplay(int amount) {
            var position = coinsSpawnPoint.position + 
                           new Vector3(Random.Range(-coinSpawnVariation, coinSpawnVariation),
                                       Random.Range(0f, coinSpawnVariation), Random.Range(-coinSpawnVariation, coinSpawnVariation));

            var rotation = Quaternion.Euler(0f, Random.Range(0f, 270f), 0f);
            
            if(amount < 100) {
                Instantiate(coinPrefab, position, rotation, coinsSpawnPoint);
                purchaseEventEmitter.SetParameter("Purchase_Size", 0f);
                purchaseEventEmitter.Play();
                return;
            }
            
            if(amount < 500) {
                Instantiate(coinStackPrefab, position, rotation, coinsSpawnPoint);
                purchaseEventEmitter.SetParameter("Purchase_Size", 1f);
                purchaseEventEmitter.Play();
                return;
            }

            Instantiate(coinMountainPrefab, position, rotation, coinsSpawnPoint);
            purchaseEventEmitter.SetParameter("Purchase_Size", 2f);
            purchaseEventEmitter.Play();
        }
        
        /// <summary>
        /// Adds more coins to the stack.
        /// </summary>
        private void RemoveCoinsDisplay() {
            for(var i = 0; i < coinsSpawnPoint.childCount; i++) {
                Destroy(coinsSpawnPoint.GetChild(0).gameObject);
            }
        }

        #endregion

        #region NPC Control

        /// <summary>
        /// Spawns the npcs while there are still items in store.
        /// </summary>
        private IEnumerator SpawnNPCs() {
            var maxNpcSpawn = ItemsToSell.Count * 2;
            
            while(ItemsToSell.Count > 0) {
                if(SpawnedNpcs.Count >= maxNpcSpawn || SpawnedNpcs.Count >= maxSpawnedNpcs) {
                    yield return spawnWait;
                    continue;
                }
                
                var npc = GameMaster.Instance.PlayerStats.Level > 10 ? npcStats[Random.Range(0, npcStats.Count)] : npcStats[Random.Range(0, 1)];
                var npcController = Instantiate(npc.npcsVariationsToUse[Random.Range(0, npc.npcsVariationsToUse.Count)],
                                                npcSpawnPoints[Random.Range(0, npcSpawnPoints.Count)].position,
                                                Quaternion.identity).GetComponent<NpcController>();

                npcController.Stats = npc;
                npcController.SetNpcsValuesBasedOnStats();
                SpawnedNpcs.Add(npcController);

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
            if(item == null) return null;
            ItemsToSell.Remove(item);
            return item;
        }
        
        /// <summary>
        /// Used by the npcs to buy an item.
        /// </summary>
        public int BuyItem(ItemStore item) {
            var itemToRemove = new InventoryItemEntry(item.Settings);
            player.Inventory.RemoveItemEntry(itemToRemove);
            player.AddCoins(item.Price);
            ProfitFromLastSale += item.Price;
            AddCoinsToDisplay(item.Price);
            SoldItems.Add(item.Settings);
            return item.Price;
        }

        /// <summary>
        /// Removes a npc from the game and list of npcs.
        /// </summary>
        public void RemoveNpc(NpcController controller) {
            SpawnedNpcs.Remove(controller);
            SpawnedNpcs.TrimExcess();
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
            
            priceSlider.maxValue = (Mathf.InverseLerp(1, player.MaxLevel, player.Level) * 0.7f) + 1.3f;

            foreach(var eventSystem in FindObjectsOfType<EventSystem>()) {
                eventSystem.SetSelectedGameObject(firstSelected);
            }
            
            UpdateItemDisplays();
            
            DOTween.To(() => mainCanvasGroup.alpha, x => mainCanvasGroup.alpha = x, 1f, fadeAnimationTime * 0.5f)
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
            
            var totalPrice = StoreInventory.ItemsInInventory.Sum(x => x.ItemSettings.initialValue * priceMarkup);

            DOTween.To(x => priceText.text = Mathf.RoundToInt(x).ToString(CultureInfo.InvariantCulture), int.Parse(priceText.text), totalPrice, Time.deltaTime * 2f);
        }

        /// <summary>
        /// Adds an item to the store inventory and removes it from the player's.
        /// </summary>
        private void AddToStoreInventory(InventoryItemEntry itemEntry) {
            if(StoreInventory.ItemsInInventory.Count >= storeInventorySize) return;
            StoreInventory.AddItemEntry(itemEntry, 1, false);
            PlayerDuplicateInventory.RemoveItemEntry(itemEntry);
            UpdateItemDisplays();
        }
        
        /// <summary>
        /// Removes an item from the store inventory and adds it to the player's.
        /// </summary>
        private void RemoveFromStoreInventory(InventoryItemEntry itemEntry) {
            if(PlayerDuplicateInventory.ItemsInInventory.Count >= PlayerDuplicateInventory.InventorySize) return;
            PlayerDuplicateInventory.AddItemEntry(itemEntry, 1, false);
            StoreInventory.RemoveItemEntry(itemEntry);
            UpdateItemDisplays();
        }
        
        /// <summary>
        /// Hides the store UI and allows the player to move again.
        /// </summary>
        public void HideStoreUi(bool unlockPlayer = true) {
            DOTween.To(() => mainCanvasGroup.alpha, x => mainCanvasGroup.alpha = x, 0f, fadeAnimationTime).onComplete +=
                () => {
                    player.GetComponent<NavMeshAgent>().enabled = unlockPlayer;
                    player.CanMoveOverride = unlockPlayer;
                    if(unlockPlayer) canvasTrigger.ResetTrigger();
                };
        }
        
        #endregion
    }
}