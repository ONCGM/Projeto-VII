using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items {
    /// <summary>
    /// Holds info on an item in the player inventory.
    /// </summary>
    public struct InventoryItemEntry {
        /// <summary>
        /// What item is this.
        /// </summary>
        public ItemSettings ItemSettings { get; private set; }
        
        /// <summary>
        /// How many of this item.
        /// </summary>
        public int Stack { get; private set; }
        
        public InventoryItemEntry(ItemSettings itemSettings, int stack) {
            Stack = stack;
            ItemSettings = itemSettings;
        }

        public void AddToStack(int amount) {
            Stack += amount;
        }
    }
}