using System;
using System.Collections.Generic;
using Entity.Player;
using Game;
using Items;
using Localization;

namespace Utility {
    /// <summary>
    /// This class is meant to hold information relevant to the game. A.K.A a save file.
    /// Fill in all the variables you need, I've only placed a few general variables,
    /// because I don't know what you will need in your game. 
    /// </summary>
    [Serializable]
    public class SaveData {
        /// <summary>
        /// The current stats of the player.
        /// </summary>
        public PlayerStats currentPlayerStats = new PlayerStats() {
            Health = 35, MaxHealth = 35, Stamina = 20, MaxStamina = 20,
            MeleeDamage = 7, RangedDamage = 5, MovementSpeed = 15,
            Level = 1, Experience = 0, TotalExperience = 0,
            Coins = 0, CurrentInventory = new List<InventoryItemEntry>(),
            CurrentUpgradeLevel = 0
        };

        /// <summary>
        /// Current player inventory ids.
        /// </summary>
        public List<int> currentInventoryIds = new List<int>();

        /// <summary>
        /// The in game day the player is in.
        /// </summary>
        public int gameDay = 1;

        /// <summary>
        /// The current time of day the player is in.
        /// </summary>
        public TimeOfDay currentTimeOfDay = TimeOfDay.Morning;
        
        /// <summary>
        /// How many times has the player died.
        /// </summary>
        public int playerDeathCount = 0;

        /// <summary>
        /// List of all important dialogs. True if the player has seen them.
        /// </summary>
        public List<bool> dialogsCleared = new List<bool>();

        /// <summary>
        /// The game difficulty set by the player.
        /// </summary>
        public float difficulty = 1f;

        /// <summary>
        /// The language that the player is currently using.
        /// </summary>
        public LocalizationSystem.Language currentLanguage = LocalizationSystem.Language.Portuguese_Brazil;

        /// <summary>
        /// The graphics quality level. See Unity Quality Settings for more information on how to use this. 
        /// </summary>
        public bool graphicsLevel = true;
        
        /// <summary>
        /// Is the game running in fullscreen mode.
        /// </summary>
        public bool fullscreen = true;

        /// <summary>
        /// The audio mixer master channel volume. 
        /// </summary>
        public float audioMasterVolume = 0.8f;

        /// <summary>
        /// The audio mixer music channel volume.
        /// </summary>
        public float audioMusicVolume = 0.8f;
        
        /// <summary>
        /// The audio mixer sfx channel volume.
        /// </summary>
        public float audioSfxVolume = 0.8f;

        /// <summary>
        /// Changed to false as soon as the player or the auto-save
        /// firsts save a file in game.
        /// </summary>
        public bool brandSpankingNewSave = true;
    }
}