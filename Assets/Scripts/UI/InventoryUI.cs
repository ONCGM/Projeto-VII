using System.Collections;
using System.Collections.Generic;
using Entity.Player;
using Items;
using UnityEngine;

namespace UI {
    public class InventoryUI : MonoBehaviour {
        [Header("Player Reference")]
        [SerializeField] private PlayerController player;

        public PlayerController Player {
            get => player;
            set => player = value;
        }

        [Header("Prefabs")]
        [SerializeField] private GameObject itemUiPrefab;

        /// <summary>
        /// Finds references and sets up the class.
        /// </summary>
        private void Start() {
            if(player is null) player = GameObject.FindObjectOfType<PlayerController>();
            player.Inventory.OnInventoryUpdate.AddListener(UpdateItems);
        }

        /// <summary>
        /// Updates the items in the ui.
        /// </summary>
        private void UpdateItems() {
            for(int i = 0; i < transform.childCount; i++) {
                Destroy(transform.GetChild(i).gameObject);
            }

            foreach(var item in Player.Inventory.ItemsInInventory) {
                Instantiate(itemUiPrefab, transform).GetComponent<ItemUI>().SetUpItemUi(item);
            }
        }
    }
}