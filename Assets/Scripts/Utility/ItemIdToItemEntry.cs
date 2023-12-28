using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEngine;

namespace Utility {
    /// <summary>
    /// Converts list of item ids or item id to an item entry or list of item entries.
    /// </summary>
    [CreateAssetMenu(fileName = "Item_Converter", menuName = "Scriptable Objects/Item Converter")]
    public class ItemIdToItemEntry : ScriptableObject {
        #pragma warning disable 0649
        [Header("Settings")]
        [SerializeField] private List<ItemSettings> items = new List<ItemSettings>();
        #pragma warning restore 0649

        /// <summary>
        /// Returns the settings for the item based on id.
        /// </summary>
        public ItemSettings ReturnSettingFromId(int id) => items.Find(x => x.itemId == id);

        /// <summary>
        /// Returns a list of the settings for the items based on id.
        /// </summary>
        public List<ItemSettings> ReturnSettingFromIds(IEnumerable<int> ids) => ids.Select(id => items.Find(x => x.itemId == id)).ToList();
        
        /// <summary>
        /// Returns a list of item entries for the items based on id.
        /// </summary>
        public List<InventoryItemEntry> ReturnEntriesFromIds(IEnumerable<int> ids) => ids.Select(id => new InventoryItemEntry(items.Find(x => x.itemId == id))).ToList();
        
        /// <summary>
        /// Returns the id for the item based on settings.
        /// </summary>
        public int ReturnIdFromSettings(ItemSettings setting) => setting.itemId;

        /// <summary>
        /// Returns a list of the ids for the items based on settings.
        /// </summary>
        public List<int> ReturnIdsFromSettings(IEnumerable<ItemSettings> settings) => settings.Select(itemSettings => itemSettings.itemId).ToList();
        
        /// <summary>
        /// Returns a list of the id for the items based on entries.
        /// </summary>
        public static List<int> ReturnIdsFromEntries(IEnumerable<InventoryItemEntry> entries) => entries.Select(entry => entry.ItemSettings.itemId).ToList();
    }
}