using System;
using Entity.Player;
using Game;
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
        public PlayerStats currentPlayerStats = new PlayerStats();

        /// <summary>
        /// The in game day the player is in.
        /// </summary>
        public int gameDay = 0;

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
        public bool[] dialogsCleared = new[] {false, false, false};

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
        public float audioMasterVolume = 0f;

        /// <summary>
        /// The audio mixer music channel volume.
        /// </summary>
        public float audioMusicVolume = 0f;
        
        /// <summary>
        /// The audio mixer sfx channel volume.
        /// </summary>
        public float audioSfxVolume = 0f;

        /// <summary>
        /// The total amount of time the player spent playing the game.
        /// </summary>
        public float totalTimeInGame = 0f;
    }
}