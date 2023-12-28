using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using UI.Popups;
using UnityEngine;
using UnityEngine.Events;

namespace Items {
    /// <summary>
    /// Class for holding items. Used by the player, npcs and the store.
    /// </summary>
    [System.Serializable]
    public class Inventory {
        #pragma warning disable 0649
        // Fields and Properties.
        public int InventorySize { get; set; }

        public List<InventoryItemEntry> ItemsInInventory { get; set;}
        
        // Localization.
        private const string inventoryFullTitleKey = "GAME_INVENTORY";
        private const string inventoryFullMessageKey = "GAME_INVENTORY_FULL";
        private const string okKey = "MENU_LABEL_OK";
        private List<CanvasPopupDialog.ButtonSettings> buttons = new List<CanvasPopupDialog.ButtonSettings>();
        private const string popUpPrefabPath = "Prefabs/UI/Popups/Pop-Up Canvas";
        private GameObject popupPrefab;
        private CanvasPopupDialog popupDialog;
        private bool configured;
        
        // Events.
        public UnityEvent OnInventoryUpdate = new UnityEvent();

        #pragma warning restore 0649
        
        public Inventory(int inventorySize, List<InventoryItemEntry> items) {
            InventorySize = inventorySize;
            ItemsInInventory = new List<InventoryItemEntry>(items);
        }

        /// <summary>
        /// Configures the popup for full inventory.
        /// </summary>
        private void SetUpPopup() {
            configured = true;
            popupPrefab = Resources.Load<GameObject>(popUpPrefabPath);
            buttons = new List<CanvasPopupDialog.ButtonSettings> {
                new CanvasPopupDialog.ButtonSettings(okKey, CanvasPopupDialog.PopupButtonHighlight.Normal, 0)
            };
        }

        /// <summary>
        /// Instantiates a popup.
        /// </summary>
        private void FullInventoryPopup() {
            if(popupDialog != null) return;
            SetUpPopup();
            popupDialog = GameObject.Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();
            popupDialog.SetUpPopup(inventoryFullTitleKey, inventoryFullMessageKey, 
                                   buttons, ExecutionState.PopupPause, i => { });
        }

        /// <summary>
        /// Adds an item to the inventory if there's space available.
        /// </summary>
        /// <param name="item"> Item to add.</param>
        /// <param name="amount"> How many to add.</param>
        /// <param name="fullPopup"> Show popup? </param>
        public bool AddItemEntry(InventoryItemEntry item, int amount = 1, bool fullPopup = true) {
            if(!configured) SetUpPopup();

            // while(amount > 0) {
            //     if(ItemsInInventory.Exists(x => (x.ItemSettings.itemId == item.ItemSettings.itemId) && (x.Stack < item.ItemSettings.maxStackQuantity))) {
            //
            //         var itemEntry = ItemsInInventory.First(x => (x.ItemSettings.itemId == item.ItemSettings.itemId) && (x.Stack < item.ItemSettings.maxStackQuantity));
            //         
            //         //var freeSpaceInStack = (item.ItemSettings.maxStackQuantity - itemEntry.Stack);
            //
            //         //var amountToAddToStack = Mathf.Min(amount, freeSpaceInStack);
            //
            //         itemEntry.AddToStack(1);
            //
            //         amount--;
            //         
            //         OnInventoryUpdate?.Invoke();
            //     } else {
            //         if(ItemsInInventory.Count < InventorySize) {
            //             ItemsInInventory.Add(new InventoryItemEntry(item.ItemSettings));
            //             amount--;
            //             OnInventoryUpdate?.Invoke();
            //         } else {
            //             if(fullPopup) FullInventoryPopup();
            //             OnInventoryUpdate?.Invoke();
            //             return false;
            //         }
            //     }
            // }
            
            if(ItemsInInventory.Count < InventorySize) {
                ItemsInInventory.Add(new InventoryItemEntry(item.ItemSettings));
                OnInventoryUpdate?.Invoke();
            } else {
                if(fullPopup) FullInventoryPopup();
                OnInventoryUpdate?.Invoke();
                return false;
            }
            
            OnInventoryUpdate?.Invoke();
            return true;
        }
        
        /// <summary>
        /// Removes an item to the inventory if it exists and returns it. Will return null if can't find an item.
        /// </summary>
        /// <param name="item"> Item to remove.</param>
        public InventoryItemEntry? RemoveItemEntry(InventoryItemEntry item) {
            OnInventoryUpdate?.Invoke();
            if(!ItemsInInventory.Exists(x => (x.ItemSettings.itemId == item.ItemSettings.itemId))) return null;
            var itemEntry = ItemsInInventory.First(x => 
                                                       (x.ItemSettings.itemId == item.ItemSettings.itemId));

            if(itemEntry.Stack > 1) {
                ItemsInInventory.First(x => (x.ItemSettings.itemId == item.ItemSettings.itemId) && (x.Stack > 1))
                                .AddToStack(-1);
            } else {
                ItemsInInventory.Remove(itemEntry);
            }
            
            OnInventoryUpdate?.Invoke();
            return itemEntry;
        }
    }
}