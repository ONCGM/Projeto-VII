using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items {
    /// <summary>
    /// Holds info on an item in the player inventory.
    /// </summary>
    [Serializable]
    public struct InventoryItemEntry {
        /// <summary>
        /// What item is this.
        /// </summary>
        public ItemSettings ItemSettings { get; private set; }
        
        /// <summary>
        /// How many of this item.
        /// </summary>
        public int Stack { get; private set; }
        
        public InventoryItemEntry(ItemSettings itemSettings, int stack = 1) {
            Stack = stack;
            ItemSettings = itemSettings;
        }

        public void AddToStack(int amount) {
            Stack += amount;
        }
    }
}