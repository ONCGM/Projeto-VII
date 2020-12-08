using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entity.Player;
using Items;
using JetBrains.Annotations;
using Localization;
using Ship;
using UnityEngine;
using Utility;

namespace Game {
    /// <summary>
    /// Singleton class to hold and control information between game scenes.
    /// </summary>
    public class GameMaster {
        
        private static readonly GameMaster instance = new GameMaster();

        static GameMaster() { }

        private GameMaster() {
            ItemConverter = Resources.Load<ItemIdToItemEntry>(ItemConverterPath);
            SavingIconPrefab = Resources.Load<GameObject>(SavingIconPath);
            SaveSystem.LoadGameFile();
            MasterSaveData = SaveSystem.LoadedData;
            PlayerStats = MasterSaveData.currentPlayerStats;
            playerStats.CurrentInventory = ItemConverter.ReturnEntriesFromIds(MasterSaveData.currentInventoryIds);
            CurrentGameDay = MasterSaveData.gameDay;
            CurrentTimeOfDay = MasterSaveData.currentTimeOfDay;
            DialogsCleared = MasterSaveData.dialogsCleared;
            GameDifficulty = MasterSaveData.difficulty;
            LocalizationSystem.CurrentLanguage = MasterSaveData.currentLanguage;
        }

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
                OnGameExecutionStateUpdated?.Invoke(gameState);
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

        private PlayerStats playerStats = new PlayerStats() {
            Health = 35, MaxHealth = 35, Stamina = 20, MaxStamina = 20,
            MeleeDamage = 7, RangedDamage = 5, MovementSpeed = 15,
            Level = 1, Experience = 0, TotalExperience = 0,
            Coins = 0, CurrentInventory = new List<InventoryItemEntry>(),
            CurrentUpgradeLevel = 0
        };

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
        /// Player stats when he arrived at island.
        /// </summary>
        public PlayerStats PlayerStatsBeforeIsland { get; set; }

        /// <summary>
        /// Important dialogs that the player has seen.
        /// </summary>
        public List<bool> DialogsCleared { get; set; } = new List<bool>();

        private TimeOfDay currentTimeOfDay = TimeOfDay.Morning;

        /// <summary>
        /// What time is it in game.
        /// </summary>
        public TimeOfDay CurrentTimeOfDay {
            get => currentTimeOfDay;
            set {
                currentTimeOfDay = value;
                OnTimeOfDayUpdated?.Invoke();
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
                if(currentGameDay < 10) return;
                GameSceneWasLoaded = true;
                OnEndGameDayUpdate?.Invoke();
            }
        }

        private float gameDifficulty = 1f;

        /// <summary>
        /// The current game difficulty.
        /// </summary>
        public float GameDifficulty {
            get => gameDifficulty;
            set => gameDifficulty = Mathf.Clamp(value, 0.5f, 1.5f);
        }

        /// <summary>
        /// Returns the inverse of the current difficulty.
        /// If its maxed out, will return minimum amount.
        /// </summary>
        public float GameDifficultyReversed => Mathf.Lerp(1.5f, 0.5f,
                     Mathf.InverseLerp(0.5f, 1.5f, gameDifficulty));

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
        /// The current type of island by random choice. 
        /// Set by the island level loader and used by the island scripts.
        /// </summary>
        public IslandType CurrentIslandType { get; set; } = IslandType.BrawlIsland;
        
        /// <summary>
        /// The current spawned ship in the game world.
        /// </summary>
        public ShipTravelController ShipTravel { get; set; }

        /// <summary>
        /// Used in the credits scene, to go back to the correct place.
        /// </summary>
        public bool GameSceneWasLoaded { get; set; }

        #region Actions

        /// <summary>
        /// Called whenever the PlayerStats are updated.
        /// </summary>
        public static Action OnPlayerStatsUpdated;
        
        /// <summary>
        /// Called whenever the game execution state is updated.
        /// </summary>
        public static Action<ExecutionState> OnGameExecutionStateUpdated;
        
        /// <summary>
        /// Called whenever the game menu is open or closed.
        /// </summary>
        public static Action OnGameMenuUpdated;

        /// <summary>
        /// Called whenever the in game time changes.
        /// </summary>
        public static Action OnTimeOfDayUpdated;

        /// <summary>
        /// Called whenever a in game day passes.
        /// </summary>
        public static Action OnGameDayUpdate;
        
        /// <summary>
        /// Called when the last game day starts and ends.
        /// </summary>
        public static Action OnEndGameDayUpdate;

        /// <summary>
        /// Invoked just before the menu scene starts to load when returning to the main menu.
        /// </summary>
        public static Action OnReturnToMenu;
        
        /// <summary>
        /// Invoked just as the credits scene is loaded.
        /// </summary>
        public static Action OnCreditsOpen;

        /// <summary>
        /// Called when a save file is loaded.
        /// </summary>
        public static Action OnSaveDataUpdated;

        /// <summary>
        /// Called whenever the game is saved automatically.
        /// </summary>
        public static Action OnGameSaved;
        
        #endregion

        #region Time Stuff

        /// <summary>
        /// Passes time by one period.
        /// </summary>
        public void AdvanceOneTimePeriod() {
            switch(CurrentTimeOfDay) {
                case TimeOfDay.Morning:
                    CurrentTimeOfDay = TimeOfDay.Afternoon;
                    break;
                case TimeOfDay.Afternoon:
                    CurrentTimeOfDay = TimeOfDay.Night;
                    break;
                case TimeOfDay.Night:
                    CurrentTimeOfDay = TimeOfDay.Morning;
                    CurrentGameDay++;
                    break;
            }
            
            SaveGame();
        }

        #endregion
        
        #region Saving
        
        /// <summary>
        /// The save date in use.
        /// </summary>
        public SaveData MasterSaveData { get; private set; }

        /// <summary>
        /// Path to the item converter scriptable object.
        /// </summary>
        public string ItemConverterPath = "Scriptables/Items/Item_Converter";

        /// <summary>
        /// Converts items and ids for serializing.
        /// </summary>
        public ItemIdToItemEntry ItemConverter { get; private set; }

        /// <summary>
        /// Path to the saving icon prefab object.
        /// </summary>
        public string SavingIconPath = "Prefabs/UI/Saving Icon Canvas";
        
        /// <summary>
        /// The saving icon prefab.
        /// </summary>
        public GameObject SavingIconPrefab { get; private set; }

        /// <summary>
        /// Last time that the game was saved.
        /// </summary>
        private float lastTimeOfSave = 0f;
        
        /// <summary>
        /// How often to save.
        /// </summary>
        private const float delayBetweenSaves = 45f;

        /// <summary>
        /// Overrides save data.
        /// </summary>
        public void SetSaveData(SaveData save) {
            MasterSaveData = save;
            OnSaveDataUpdated?.Invoke();
        }

        /// <summary>
        /// Save the game to a file.
        /// </summary>
        public void SaveGame() {
            MasterSaveData.currentPlayerStats = PlayerStats;
            MasterSaveData.gameDay = CurrentGameDay;
            MasterSaveData.dialogsCleared = DialogsCleared;
            if(MasterSaveData.currentPlayerStats.CurrentInventory != null &&
               MasterSaveData.currentPlayerStats.CurrentInventory.Count > 0) {
                MasterSaveData.currentInventoryIds = ItemIdToItemEntry.ReturnIdsFromEntries(MasterSaveData.currentPlayerStats.CurrentInventory);
            }
            SaveSystem.LoadedData = MasterSaveData;

            if(Time.time < lastTimeOfSave + delayBetweenSaves) return;
            SaveSystem.SerializeToFile();
            OnGameSaved?.Invoke();
            GameObject.Instantiate(SavingIconPrefab);
        }
        
        #endregion
    }

    #region Enums
    
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
        Morning,
        Afternoon,
        Night
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
    
    #endregion
}