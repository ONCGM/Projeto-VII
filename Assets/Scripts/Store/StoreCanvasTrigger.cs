using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Store {
    /// <summary>
    /// Trigger an event to open the store canvas.
    /// </summary>
    public class StoreCanvasTrigger : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")] [SerializeField] private bool canOpen = true;
        [SerializeField] private string playerTag = "Player";
        private StoreController controller; 
        
        #pragma warning restore 0649

        // Gets references.
        private void Start() => controller = GetComponentInParent<StoreController>();

        /// <summary>
        /// Resets the state and allows the canvas to open up again.
        /// </summary>
        public void ResetTrigger() => canOpen = true;

        // Collision detection.
        private void OnTriggerEnter(Collider other) {
            if(!other.CompareTag(playerTag) || !canOpen) return;
            controller.DisplayStoreUi();
            canOpen = false;
        }
    }
}