using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Entity;
using Entity.Player;
using UnityEngine;
using FMODUnity;

namespace Items {
    /// <summary>
    /// Item drop base class, uses info from a scriptable object to set the item model and stats.
    /// </summary>
    [RequireComponent( typeof(StudioEventEmitter), typeof(Collider))]
    public class ItemDrop : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Animation ItemSettings")]
        [SerializeField, Range(0.1f, 3f)] private float jumpHeight = 0.75f;
        [SerializeField, Range(0.1f, 5f)] private float jumpTime = 1.5f;

        [Header("Item ItemSettings")] 
        [SerializeField] private ItemSettings settings;

        [Header("Collision ItemSettings")]
        [SerializeField] private string playerTag = "Player";
        
        // Components
        private StudioEventEmitter eventEmitter;

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
             ItemIdleAnimation();
             eventEmitter = GetComponent<StudioEventEmitter>();
        }

        /// <summary>
        /// Sets the item info and spawns it's 3D model.
        /// </summary>
        public void SetItemBasedOnSettings(ItemSettings itemSettings) {
            settings = itemSettings;

            if(transform.childCount > 0) {
                for(int i = 0; i < transform.childCount; i++) {
                    Destroy(transform.GetChild(i));
                }
            }
            
            Instantiate(settings.itemModel, transform);
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
            var stats = new InventoryItemEntry(settings, 1);

            player.Inventory.AddItemEntry(stats);

            foreach(var item in player.Inventory.ItemsInInventory) {
                Debug.Log(item.ItemSettings.itemName);
            }
            
            // TODO: Add particles, sfx and effects.
            eventEmitter.Play();
            DOTween.Kill(this);
            Destroy(gameObject);
        }
    }
}