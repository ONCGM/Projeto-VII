using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Items {
    /// <summary>
    /// Class for holding items. Only used by the player so far.
    /// </summary>
    public class Inventory {
        // Fields and Properties.
        public int InventorySize { get; set; }

        public List<InventoryItemEntry> ItemsInInventory { get; private set;}
        
        // Events.
        public UnityEvent OnInventoryUpdate = new UnityEvent();

        public Inventory(int inventorySize, List<InventoryItemEntry> items) {
            InventorySize = inventorySize;
            ItemsInInventory = items;
        }

        /// <summary>
        /// Adds an item to the inventory if there's space available.
        /// </summary>
        /// <param name="item"> Item to add.</param>
        /// <param name="amount"> How many to add.</param>
        public void AddItemEntry(InventoryItemEntry item, int amount = 1) {
            while(amount > 0) {
                if(ItemsInInventory.Exists(x => (x.ItemSettings.itemId == item.ItemSettings.itemId) &&
                                                (x.Stack < item.ItemSettings.maxStackQuantity))) {
                    var itemEntry = ItemsInInventory.First(x => 
                                                                     (x.ItemSettings.itemId == item.ItemSettings.itemId) &&
                                                                      (x.Stack < item.ItemSettings.maxStackQuantity));
                    
                    var freeSpaceInStack = (item.ItemSettings.maxStackQuantity - itemEntry.Stack);

                    var amountToAddToStack = Mathf.Min(amount, freeSpaceInStack);

                    itemEntry.AddToStack(amountToAddToStack);
                    
                    amount -= amountToAddToStack;
                    
                    OnInventoryUpdate?.Invoke();
                } else {
                    if(ItemsInInventory.Count < InventorySize) {
                        ItemsInInventory.Add(new InventoryItemEntry(item.ItemSettings, 0));
                    } else {
                        // TODO: Out of space pop-up.
                    }
                    
                    OnInventoryUpdate?.Invoke();
                }
            }
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

            if(item.Stack > 1) {
                item.AddToStack(-1);
            } else {
                ItemsInInventory.Remove(itemEntry);
            }
            
            OnInventoryUpdate?.Invoke();
            return itemEntry;
        }
    }
}