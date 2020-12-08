using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using Game;
using Items;
using UnityEngine;

namespace Store {
    /// <summary>
    /// An item version for store and npc use.
    /// </summary>
    public class ItemStore : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Item Settings")] 
        [SerializeField] private ItemSettings settings;

        // Components

        /// <summary>
        /// The item settings of this item.
        /// </summary>
        public ItemSettings Settings {
            get => settings;
            set {
                settings = value;
                SetItemBasedOnSettings(settings);
            }
        }
        
        /// <summary>
        /// Price of this item.
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// Where to pick this item.
        /// </summary>
        public Transform itemWaypoint { get; set; }

        private StoreController store;
        
        #pragma warning restore 0649

        // Sets up the item.
        private void Awake() => store = FindObjectOfType<StoreController>();

        /// <summary>
        /// Sets the item info and spawns it's 3D model.
        /// </summary>
        public void SetItemBasedOnSettings(ItemSettings itemSettings) {
            settings = itemSettings;

            for(var i = 0; i < transform.childCount; i++) Destroy(transform.GetChild(i));
            
            Instantiate(settings.itemModel, transform);

            Price = Mathf.RoundToInt(settings.initialValue * store.PriceMarkup);

            // transform.position, Quaternion.Euler(Vector3.zero)
        }
    }
}