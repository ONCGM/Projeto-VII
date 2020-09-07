using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items {
    /// <summary>
    /// Holds information on items and their parameters.
    /// </summary>
    [CreateAssetMenu(fileName = "Item_Settings_", menuName = "Scriptable Objects/Item")]
    public class ItemSettings : ScriptableObject {
        // TODO: Add lock based on progress, so only spawns after a certain amount of progress.
        // TODO: Add localization in names, description and etc.
        [Header("Drop ItemSettings")] 
        [SerializeField] public GameObject itemPrefab;
        [SerializeField] public GameObject itemModel;

        [Header("Inventory ItemSettings")] 
        [SerializeField]public int itemId;
        [SerializeField] public string itemName;
        [SerializeField] public string itemDescription;
        [SerializeField] public string itemEffects;
        [SerializeField, Range(1, 99)] public int maxStackQuantity;
        [SerializeField] public Sprite itemImage;

        // TODO: Set price values for items.
        [Header("Price ItemSettings")] 
        [SerializeField] public int initialValue;
    }
}