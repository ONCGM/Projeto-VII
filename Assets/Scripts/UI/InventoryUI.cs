using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Player;
using Items;
using UnityEngine;

namespace UI {
    public class InventoryUI : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Player Reference")]
        [SerializeField] private PlayerController player;

        public PlayerController Player {
            get => player;
            set => player = value;
        }

        [Header("Prefabs")]
        [SerializeField] private GameObject itemUiPrefab;
        #pragma warning restore 0649
        
        
        /// <summary>
        /// Finds references and sets up the class.
        /// </summary>
        private void Start() {
            if(player == null) player = GameObject.FindObjectOfType<PlayerController>();
            player.Inventory.OnInventoryUpdate.AddListener(UpdateItems);
            UpdateItems();
            
            InvokeRepeating(nameof(UpdateItems), 2f, 5f);
        }

        // Cancel invoke.
        private void OnDestroy() => CancelInvoke(nameof(UpdateItems));

        /// <summary>
        /// Updates the items in the ui.
        /// </summary>
        private void UpdateItems() {
            if(player == null) {
                player = GameObject.FindObjectOfType<PlayerController>();
                player.Inventory.OnInventoryUpdate.AddListener(UpdateItems);
                return;
            }

            for(var i = 0; i < transform.childCount; i++) {
                Destroy(transform.GetChild(i).gameObject);
            }

            foreach(var item in Player.Inventory.ItemsInInventory) {
                Instantiate(itemUiPrefab, transform).GetComponent<ItemUI>().SetUpItemUi(item);
            }
        }
    }
}