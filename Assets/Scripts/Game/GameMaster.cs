using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Player;
using UnityEngine;

namespace Game {
    /// <summary>
    /// Singleton class to hold and control information between game scenes.
    /// </summary>
    public class GameMaster {
        
        private static readonly GameMaster instance = new GameMaster();

        static GameMaster() { }

        private GameMaster() { }

        /// <summary>
        /// The instance of this singleton.
        /// </summary>
        public static GameMaster Instance {
            get {
                return instance;
            }
        }
        

        private ExecutionState gameState;

        /// <summary>
        /// Current state of the game.
        /// </summary>
        public ExecutionState GameState {
            get => gameState;
            set {
                gameState = value;
                OnGameStateUpdated?.Invoke();
            }
        }

        private PlayerStats playerStats;

        /// <summary>
        /// Current stats of the player.
        /// </summary>
        public PlayerStats PlayerStats {
            get => playerStats;
            set {
                playerStats = value;
                OnPlayerStatsUpdated?.Invoke();
            }
            
        }
        
        /// <summary>
        /// Called whenever the PlayerStats are updated.
        /// </summary>
        public static Action OnPlayerStatsUpdated;
        
        /// <summary>
        /// Called whenever the PlayerStats are updated.
        /// </summary>
        public static Action OnGameStateUpdated;
        
        //TODO
        public object SaveData { get; private set; }

        public void SetSaveData(object saveData) {
            SaveData = saveData;
        }
    }
    
    /// <summary>
    /// Current execution state of the game.
    /// <para> Used to stop enemies if player gets a popup and
    /// similar scenarios.</para>
    /// </summary>
    public enum ExecutionState {
        Normal,
        PopupPause,
        FullPause
    }
}