using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Player;
using UnityEngine;

namespace Store {
    /// <summary>
    /// Controls the store behaviour and shit.
    /// </summary>
    public class StoreController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("References")] 
        [SerializeField] private PlayerController player;
        [SerializeField] private CanvasGroup mainCanvasGroup;
        #pragma warning restore 0649

        // Gets references and shit.
        private void Awake() {
            player = FindObjectOfType<PlayerController>();
        }
        
        /// <summary>
        /// Starts the store sequence.
        /// Skips a time period and plays the animations for
        /// the npcs and their behaviour entering in the store.
        /// </summary>
        public void OpenStore() {
            
        }
        
        /// <summary>
        /// Ends the store sequence.
        /// Saves the game and plays the animations for
        /// the npcs and their behaviour leaving the store.
        /// </summary>
        public void CloseStore() {
            
        }

        #region UI
        /// <summary>
        /// Displays the store UI and stops player from moving.
        /// </summary>
        public void DisplayStoreUi() {
            
        }

        /// <summary>
        /// Hides the store UI and allows the player to move again.
        /// </summary>
        public void HideStoreUi() {
            
        }
        
        #endregion
    }
}