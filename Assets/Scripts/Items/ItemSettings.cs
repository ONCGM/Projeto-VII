using System.Collections;
using System.Collections.Generic;
using Entity.Player;
using UnityEngine;

namespace Items {
    /// <summary>
    /// Holds information on items and their parameters.
    /// </summary>
    [CreateAssetMenu(fileName = "Item_Settings_", menuName = "Scriptable Objects/Item")]
    public class ItemSettings : ScriptableObject {
        [Header("Drop Settings")] 
        [SerializeField] public GameObject itemPrefab;
        [SerializeField] public GameObject itemModel;
        [SerializeField] public int minimumPlayerLevelToSpawn;

        [Header("Inventory Settings")] 
        [SerializeField] public int itemId;
        [SerializeField] public string itemNameKey;
        [SerializeField] public string itemDescriptionKey;
        [SerializeField] public string itemEffects;
        [SerializeField, Range(1, 99)] public int maxStackQuantity;
        [SerializeField] public Sprite itemImage;

        [Header("Price Settings")] 
        [SerializeField] public int initialValue;
        [SerializeField, Range(0.01f, 1f)] public float itemRarity;

        [Header("Use & Effects Settings")]
        [SerializeField] public bool hasEffect;
        [SerializeField, Range(1, 50)] public int effectAmount;
        
        /// <summary>
        /// Applies this item's effect.
        /// </summary>
        public void ApplyEffect() {
            if(!hasEffect) return;
            var player = FindObjectOfType<PlayerController>();
            player.Health += effectAmount;
            player.Stamina += Mathf.RoundToInt(effectAmount * 0.5f);
            player.Inventory.RemoveItemEntry(new InventoryItemEntry(this));
        }

        /// <summary>
        /// Drops the item.
        /// </summary>
        public void DropItem() {
            var player = FindObjectOfType<PlayerController>();
            player.Inventory.RemoveItemEntry(new InventoryItemEntry(this));
            var item = Instantiate(itemPrefab, (player.transform.position + (player.transform.forward * 2f)), Quaternion.identity).GetComponent<ItemDrop>();
            item.SetItemBasedOnSettings(this);
        }
    }
}