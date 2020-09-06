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
        

        private GameState gameState;

        /// <summary>
        /// Current state of the game.
        /// </summary>
        public GameState GameState {
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
    }
}