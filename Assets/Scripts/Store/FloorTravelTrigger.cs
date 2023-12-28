using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Store {
    /// <summary>
    /// Used to detect player collision and trigger
    /// a floor change.
    /// </summary>
    public class FloorTravelTrigger : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool canTriggerTravel = true;
        
        // Actions.
        public static Action OnResetTriggerLock;
        #pragma warning restore 0649

        // Sets up the class.
        private void Awake() {
            OnResetTriggerLock += () => canTriggerTravel = true;
        }

        // Collision detection.
        private void OnTriggerEnter(Collider other) {
            if(!other.CompareTag(playerTag) || !canTriggerTravel) return;
            FloorController.OnTriggerTravel?.Invoke();
            canTriggerTravel = false;
        }

        // Clears the action.
        private void OnDestroy() {
            OnResetTriggerLock = null;
        }
    }
}