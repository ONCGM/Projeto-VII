using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Player;
using Ship;
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
        /// <para> Used to stop enemies if player gets a popup and
        /// similar scenarios.</para>
        /// </summary>
        public ExecutionState GameState {
            get => gameState;
            set {
                gameState = value;
                OnGameExecutionStateUpdated?.Invoke();
                Time.timeScale = gameState == ExecutionState.FullPause ? 0f : 1f;
            }
        }

        private bool gameMenuIsOpen;
        
        /// <summary>
        /// Has the player opened the in game menu?
        /// </summary>
        public bool GameMenuIsOpen {
            get => gameMenuIsOpen;
            set {
                gameMenuIsOpen = value;
                gameState = gameMenuIsOpen ? ExecutionState.PopupPause : ExecutionState.Normal;
                OnGameMenuUpdated?.Invoke();
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

        private TimeOfDay currentTimeOfDay = TimeOfDay.Morning;

        /// <summary>
        /// What time is it in game.
        /// </summary>
        public TimeOfDay CurrentTimeOfDay {
            get => currentTimeOfDay;
            set {
                currentTimeOfDay = value;
                OnGameTimeOfDayUpdated.Invoke();
            }
        }

        private int currentGameDay = 0;

        /// <summary>
        /// The current in game day.
        /// </summary>
        public int CurrentGameDay {
            get => currentGameDay;
            set {
                currentGameDay = value;
                OnGameDayUpdate?.Invoke();
                if(currentGameDay >= 30) {
                    // TODO: Add end game event.
                    OnEndGameDayUpdate?.Invoke();
                }
            }
        }

        /// <summary>
        /// Should the player spawn in front of the store or the port. 
        /// </summary>
        public bool SpawnInFrontOfStore { get; set; } = true;

        /// <summary>
        /// The current selected size of island by the player choice. 
        /// Set by the town travel popup and used by the island generator.
        /// </summary>
        public IslandSizes SelectedIslandSize { get; set; } = IslandSizes.Small; 
        
        /// <summary>
        /// The current spawned ship in the game world.
        /// </summary>
        public ShipTravelController ShipTravel { get; set; }
        
        /// <summary>
        /// Called whenever the PlayerStats are updated.
        /// </summary>
        public static Action OnPlayerStatsUpdated;
        
        /// <summary>
        /// Called whenever the game execution state is updated.
        /// </summary>
        public static Action OnGameExecutionStateUpdated;
        
        /// <summary>
        /// Called whenever the game menu is open or closed.
        /// </summary>
        public static Action OnGameMenuUpdated;

        /// <summary>
        /// Called whenever the in game time changes.
        /// </summary>
        public static Action OnGameTimeOfDayUpdated;

        /// <summary>
        /// Called whenever a in game day passes.
        /// </summary>
        public static Action OnGameDayUpdate;
        
        /// <summary>
        /// Called when the last game day starts and ends.
        /// </summary>
        public static Action OnEndGameDayUpdate;

        //TODO
        public object SaveData { get; private set; }

        public void SetSaveData(object saveData) {
            SaveData = saveData;
        }
    }
    
    /// <summary>
    /// Current execution state of the game.
    /// </summary>
    public enum ExecutionState {
        Normal,
        PopupPause,
        FullPause
    }

    /// <summary>
    /// Defines the periods of the day used in the game.
    /// </summary>
    public enum TimeOfDay {
        Night,
        Morning,
        Afternoon
    }

    /// <summary>
    /// Defines the sizes of island for the island loader.
    /// Also synonym with the game difficulty.
    /// </summary>
    public enum IslandSizes {
        Small,
        Medium,
        Large
    }
    
    /// <summary>
    /// Defines the types of island in the game.
    /// </summary>
    public enum IslandType {
        BrawlIsland,
        TreasureIsland,
        MerchantIsland
    }
}