using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Entity.Player;
using Items;
using Localization;
using TMPro;
using UnityEngine;
using Utility;

namespace Game {
    /// <summary>
    /// Helps with debugging the game and runs cheat commands if dev build.
    /// </summary>
    public class GameDebugHelper : MonoBehaviour {
        #pragma warning disable 0649
        private PlayerController player;
        private CanvasGroup canvasGroup;

        [Header("UI Components")] 
        [SerializeField] private TMP_Text gameStateText;
        [SerializeField] private TMP_Text languageText;
        private float fadeAnimationSpeed = 0.5f;
        
        /// <summary>
        /// Is the debug menu currently enabled.
        /// </summary>
        public bool DebugEnabled { get; set; }

        #pragma warning restore 0649

        #region Unity Event Functions

        /// <summary>
        /// Sets up the class.
        /// </summary>
        private void Awake() {
            canvasGroup = GetComponentInChildren<CanvasGroup>();
        }

        /// <summary>
        /// Enables and disables the debug system.
        /// </summary>
        private void Update() {
            if(Input.GetKeyDown(KeyCode.F1) && Input.GetKeyDown(KeyCode.Home)) {
                DebugEnabled = !DebugEnabled;
                Debug.Log($"Debug options are {(DebugEnabled ? "enabled" : "disabled")}!");
                DOTween.To(()=> canvasGroup.alpha, x=> canvasGroup.alpha = x, (DebugEnabled ? 1f : 0f), fadeAnimationSpeed);
            }

            if(!DebugEnabled) {
                DOTween.To(()=> canvasGroup.alpha, x=> canvasGroup.alpha = x, (DebugEnabled ? 1f : 0f), fadeAnimationSpeed);
                return;
            }
            
            LocalizationDebug();
            AddCoins();
            AddXp();
            AddHealth();
            ResetStats();
            AddInGameTime();
            UpdateDebuggerUi();
        }
        
        #endregion

        #region Debug Debug.

        /// <summary>
        /// Updates the debugger UI.
        /// </summary>
        private void UpdateDebuggerUi() {
            gameStateText.text = $"{GameMaster.Instance.GameState}";
            languageText.text = $"{LocalizationSystem.CurrentLanguage}";
        }
        

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
            if(Input.GetKeyDown(KeyCode.Equals)) player.AddCoins(250);
            if(Input.GetKeyDown(KeyCode.Minus)) player.AddCoins(-250);
            
            if(coins != player.Coins) Debug.Log($"Player coins changed by {player.Coins - coins}.");
        }
        
        /// <summary>
        /// Adds xp to the player.
        /// </summary>
        private void AddXp() {
            if(player == null) player = FindObjectOfType<PlayerController>();
            var xp = player.Experience;
            if(Input.GetKeyDown(KeyCode.Alpha0)) player.AddExperience(150);
            if(Input.GetKeyDown(KeyCode.Alpha9)) player.Level--;
            
            if(xp != player.Experience) Debug.Log($"Player XP changed by {player.Experience - xp}.");
        }

        /// <summary>
        /// Adds or reduces health to the player.
        /// </summary>
        private void AddHealth() {
            if(player == null) player = FindObjectOfType<PlayerController>();
            var hp = player.Health;
            if(Input.GetKeyDown(KeyCode.Alpha8)) player.Damage(-10, player);
            if(Input.GetKeyDown(KeyCode.Alpha7)) player.Damage(10, player);
            
            if(hp != player.Health) Debug.Log($"Player HP changed by {player.Health - hp}.");
        }
        
        #endregion

        #region Game Debug.

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
            
            GameMaster.Instance.DialogsCleared = new List<bool>();
            GameMaster.Instance.CurrentGameDay = 1;
            GameMaster.Instance.CurrentTimeOfDay = TimeOfDay.Morning;
            
            SaveSystem.LoadedData = new SaveData();
            GameMaster.Instance.SetSaveData(new SaveData());
            
            GameMaster.Instance.SaveGame();
            
            Debug.LogWarning("Player Stats & Save Reset.");
        }

        /// <summary>
        /// Advances the in game clock.
        /// </summary>
        private void AddInGameTime() {
            var day = GameMaster.Instance.CurrentGameDay;
            var time = GameMaster.Instance.CurrentTimeOfDay;

            if(Input.GetKeyDown(KeyCode.F8)) GameMaster.Instance.AdvanceOneTimePeriod();

            if(Input.GetKeyDown(KeyCode.F11)) {
                GameMaster.Instance.CurrentGameDay++;
                GameMaster.Instance.CurrentTimeOfDay = TimeOfDay.Morning;
            }
            
            if(day == GameMaster.Instance.CurrentGameDay && time == GameMaster.Instance.CurrentTimeOfDay) return;
            Debug.Log($"The game time was day {day} in the {time}.");
            Debug.Log($"The game time is now day {GameMaster.Instance.CurrentGameDay} in the {GameMaster.Instance.CurrentTimeOfDay}.");
        }
        
        #endregion
    }
}