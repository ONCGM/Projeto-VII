using System;
using System.Collections;
using System.Collections.Generic;
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
        [Header("Animation Item Settings")]
        [SerializeField, Range(0.1f, 3f)] private float jumpHeight = 0.75f;
        [SerializeField, Range(0.1f, 5f)] private float jumpTime = 1.5f;

        [Header("Item ItemSettings")] 
        [SerializeField] private ItemSettings settings;

        [Header("Collision ItemSettings")]
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
             ItemIdleAnimation();
             eventEmitter = GetComponent<StudioEventEmitter>();
             Invoke(nameof(CheckObjectSpawn), 1f);
        }
        
        /// <summary>
        /// Checks if it spawned the asset, in case a enemy forgot or this is a stage drop.
        /// </summary>
        protected virtual void CheckObjectSpawn() {
            if(!hasSpawned) SetItemBasedOnSettings(settings);
        }

        /// <summary>
        /// Sets the item info and spawns it's 3D model.
        /// </summary>
        public void SetItemBasedOnSettings(ItemSettings itemSettings) {
            if(hasSpawned) return;
            
            settings = itemSettings;
            
            Instantiate(settings.itemModel, transform);
            
            hasSpawned = true;
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
            player.PlayerIslandInventory.AddItemEntry(stats);

            // TODO: Add particles, sfx and effects.
            eventEmitter.Play();
            DOTween.Kill(this);
            Destroy(gameObject);
        }
    }
}