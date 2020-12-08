using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Entity;
using Entity.Player;
using UnityEngine;
using FMODUnity;
using Game;

namespace Items {
    /// <summary>
    /// Item drop base class, uses info from a scriptable object to set the item model and stats.
    /// </summary>
    [RequireComponent( typeof(StudioEventEmitter), typeof(Collider))]
    public class ItemDrop : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Animation Settings")]
        [SerializeField, Range(0.1f, 3f)] private float jumpHeight = 0.75f;
        [SerializeField, Range(0.1f, 5f)] private float jumpTime = 1.5f;

        [Header("Item Settings")] 
        [SerializeField] private ItemSettings settings;
        [SerializeField] private List<ItemSettings> itemList = new List<ItemSettings>();

        [Header("Collision Settings")]
        [SerializeField] private string playerTag = "Player";
        
        // Components
        private StudioEventEmitter eventEmitter;
        protected bool hasSpawned;

        /// <summary>
        /// The item settings of this item.
        /// </summary>
        public ItemSettings Settings {
            get => settings;
            set => settings = value;
        }

        #pragma warning restore 0649
        
        /// <summary>
        /// Sets up the item and starts playing an animation.
        /// </summary>
        protected virtual void Awake() {
            eventEmitter = GetComponent<StudioEventEmitter>();
            Invoke(nameof(CheckObjectSpawn), 1f);
        }
        
        /// <summary>
        /// Checks if it spawned the asset, in case a enemy forgot or this is a stage drop.
        /// </summary>
        protected virtual void CheckObjectSpawn() {
            if(hasSpawned) return;
            var items = itemList.Where(x => GameMaster.Instance.PlayerStats.Level >= x.minimumPlayerLevelToSpawn).ToList();
            
            if(items.Count < 1) {
                Destroy(gameObject);
                return;
            }

            var rng = Random.value;
            var possibleItems = items.Where(x => x.itemRarity >= rng).ToList();
            
            if(possibleItems.Count < 1) {
                Destroy(gameObject);
                return;
            }
            
            settings = possibleItems[Random.Range(0, possibleItems.Count)];
            SetItemBasedOnSettings(settings);
        }

        /// <summary>
        /// Sets the item info and spawns it's 3D model.
        /// </summary>
        public void SetItemBasedOnSettings(ItemSettings itemSettings) {
            if(hasSpawned) return;
            
            settings = itemSettings;
            
            Instantiate(settings.itemModel, transform.position, Quaternion.Euler(Vector3.zero), transform);
            
            hasSpawned = true;
            
            ItemIdleAnimation();
        }

        /// <summary>
        /// Animates the item bobbing up and down.
        /// </summary>
        protected virtual void ItemIdleAnimation() {
            transform.DOLocalJump(transform.position, jumpHeight, 1, jumpTime, false).SetId(this).onComplete = ItemIdleAnimation;
        }

        /// <summary>
        /// Detects if collided with the player and adds item to player inventory.
        /// </summary>
        protected void OnTriggerEnter(Collider other) {
            if(!other.CompareTag(playerTag)) return;
            if(!other.GetComponent<PlayerController>()) return;
            var player = other.GetComponent<PlayerController>();
            var stats = new InventoryItemEntry(settings);
            
            var successfullyAdded = player.Inventory.AddItemEntry(stats, 1);
            player.PlayerIslandInventory.AddItemEntry(stats, 1, false);

            // TODO: Add particles, sfx and effects.
            if(!successfullyAdded) return;
            
            DOTween.Kill(this);
            eventEmitter.Play();
            Destroy(gameObject);
        }
    }
}