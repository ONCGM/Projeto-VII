using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Player;
using Items;
using Localization;
using UnityEngine;
using Utility;

namespace Game {
    /// <summary>
    /// Helps with debugging the game and runs cheat commands if dev build.
    /// </summary>
    public class GameDebugHelper : MonoBehaviour {
        #pragma warning disable 0649
        private bool debugEnabled;
        private PlayerController player;
        
        /// <summary>
        /// Is the debug menu currently enabled.
        /// </summary>
        public bool DebugEnabled {
            get => debugEnabled;
        }

        #pragma warning restore 0649

        #region Unity Event Functions

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// Enables and disables the debug system.
        /// </summary>
        private void Update() {
            if(Input.GetKeyDown(KeyCode.F1) && Input.GetKeyDown(KeyCode.Home)) {
                debugEnabled = !debugEnabled;
                Debug.Log($"Debug options are {(debugEnabled ? "enabled" : "disabled")}!");
            }
            if(!debugEnabled) return;
            LocalizationDebug();
            AddCoins();
            AddXp();
            ResetStats();
        }
        
        #endif
        #endregion

        #region Localization Debug.

        /// <summary>
        /// Changes the game language.
        /// </summary>
        private void LocalizationDebug() {
            var lastLanguage = LocalizationSystem.CurrentLanguage;
            if(Input.GetKeyDown(KeyCode.F5)) LocalizationSystem.CurrentLanguage = LocalizationSystem.Language.Portuguese_Brazil;
            if(Input.GetKeyDown(KeyCode.F6)) LocalizationSystem.CurrentLanguage = LocalizationSystem.Language.English;
            if(Input.GetKeyDown(KeyCode.F7)) LocalizationSystem.CurrentLanguage = LocalizationSystem.Language.Japanese;
            if(LocalizationSystem.CurrentLanguage != lastLanguage) Debug.Log($"Language updated to {LocalizationSystem.CurrentLanguage.ToString()}!");
        }

        #endregion

        #region Player Debug.

        /// <summary>
        /// Adds coins to the player inventory.
        /// </summary>
        private void AddCoins() {
            if(player == null) player = FindObjectOfType<PlayerController>();
            var coins = player.Coins;
            if(Input.GetKeyDown(KeyCode.Equals)) player.AddCoins(50);
            if(Input.GetKeyDown(KeyCode.Minus)) player.AddCoins(-50);
            
            if(coins != player.Coins) Debug.Log($"Player coins changed by {player.Coins - coins}.");
        }
        
        /// <summary>
        /// Adds xp to the player.
        /// </summary>
        private void AddXp() {
            if(player == null) player = FindObjectOfType<PlayerController>();
            var xp = player.Experience;
            if(Input.GetKeyDown(KeyCode.Alpha0)) player.AddExperience(50);
            if(Input.GetKeyDown(KeyCode.Alpha9)) player.AddExperience(-50);
            
            if(xp != player.Experience) Debug.Log($"Player XP changed by {player.Experience - xp}.");
        }

        /// <summary>
        /// Resets player save.
        /// </summary>
        private void ResetStats() {
            if(!Input.GetKeyDown(KeyCode.F12) || !Input.GetKeyDown(KeyCode.Delete)) { return; }

            GameMaster.Instance.PlayerStats = new PlayerStats() {
                Health = 35, MaxHealth = 35, Stamina = 20, MaxStamina = 20,
                MeleeDamage = 7, RangedDamage = 5, MovementSpeed = 15,
                Level = 0, Experience = 0, TotalExperience = 0,
                Coins = 0, CurrentInventory = new List<InventoryItemEntry>(),
                CurrentUpgradeLevel = 0
            };
            
            player.LoadGameMasterPlayerStats();

            SaveSystem.LoadedData.currentPlayerStats = GameMaster.Instance.PlayerStats;
            
            SaveSystem.SerializeToFile();
            
            Debug.LogWarning("Player Stats & Save Reset.");
        }

        #endregion
    }
}