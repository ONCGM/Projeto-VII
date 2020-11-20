using System.Collections;
using System.Collections.Generic;
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

        [Header("Price Settings")] // TODO: Game Economy. Basic as possible.
        [SerializeField] public int initialValue;
    }
}