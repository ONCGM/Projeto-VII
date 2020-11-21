using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Entity.Player;
using Game;
using Islands;
using Town;
using UI;
using UI.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UI.Popups;
using static UI.Popups.CanvasPopupDialog.PopupButtonHighlight;

namespace Ship {
    /// <summary>
    /// Controls the ship and its travelling routine.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class ShipTravelController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Scene Index")] 
        [SerializeField] private int townSceneIndex = 3;
        [SerializeField] private int travelSceneIndex = 4;
        [SerializeField] private int islandsSceneIndex = 5;
        
        [Header("Settings")]
        [SerializeField] private string playerComponentsTag = "Player Components";
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private string townLightTag, seaLightTag, islandLightTag;

        [Header("Localization")] 
        [SerializeField] private LocalizedString travelTitleKey;

        [SerializeField] private LocalizedString travelMessageKey,
                                        timeAdvanceMessageKey,
                                        backToTownMessageKey,
                                        confirmButtonKey,
                                        cancelButtonKey,
                                        smallIslandButtonKey,
                                        mediumIslandButtonKey,
                                        largeIslandButtonKey;
        
        // Buttons.
        private List<CanvasPopupDialog.ButtonSettings> travelButtons = new List<CanvasPopupDialog.ButtonSettings> ();
        

        [Header("Prefabs")] 
        [SerializeField] private GameObject popupPrefab;
        
        // Components.
        private Animator anim;
        private CinemachineVirtualCamera shipCamera;
        private static readonly int DepartToIsland = Animator.StringToHash("DepartToIsland");
        private static readonly int DepartToTown = Animator.StringToHash("DepartToTown");
        private static readonly int ArriveAtIsland = Animator.StringToHash("ArriveAtIsland");
        private static readonly int ArriveAtTown = Animator.StringToHash("ArriveAtTown");
        private static readonly string playerSettingsPath = "Scriptables/Player/Player_Upgrade_Settings";
        private PlayerUpgradeSettings upgradeSettings;
        /// <summary>
        /// Island Loader Object.
        /// </summary>
        public IslandLevelLoader IslandLoader { get; set; }

        #pragma warning restore 0649

        // Sets the ship up.
        private void Awake() {
            anim = GetComponent<Animator>();
            shipCamera = GetComponentInChildren<CinemachineVirtualCamera>();
            DontDestroyOnLoad(gameObject);
            upgradeSettings = Resources.Load<PlayerUpgradeSettings>(playerSettingsPath);

            // Populate buttons.
            travelButtons.Add(new CanvasPopupDialog.ButtonSettings(smallIslandButtonKey.key, Normal, 0));
            travelButtons.Add(new CanvasPopupDialog.ButtonSettings(mediumIslandButtonKey.key, Normal, 1));
            travelButtons.Add(new CanvasPopupDialog.ButtonSettings(largeIslandButtonKey.key, Normal, 2));
            travelButtons.Add(new CanvasPopupDialog.ButtonSettings(confirmButtonKey.key, StrongHighlight, 3));
            travelButtons.Add(new CanvasPopupDialog.ButtonSettings(cancelButtonKey.key, Highlight, 4));

            GameMaster.OnCreditsOpen += () => {
                GameMaster.Instance.ShipTravel = null;
                Destroy(gameObject);
            };
            
            GameMaster.OnReturnToMenu += () => {
                GameMaster.Instance.ShipTravel = null;
                Destroy(gameObject);
            };
        }

        #region Travel to Islands
        
        /// <summary>
        /// Start the travel routine to the islands.
        /// </summary>
        public void StartTravelToIsland() {
            // TODO: Add to late message and lock player from travelling.
            var popup = Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();
            var player = FindObjectOfType<PlayerController>();
            FindObjectOfType<PlayerStatsUI>().ShowHideCanvasKeepClock(false);
            var buttons = new List<CanvasPopupDialog.ButtonSettings>();
            var index = 0;
            
            foreach(var islandLevelRequirement in upgradeSettings.islandSizeUnlocksAtLevel) {
                if(player.Level >= islandLevelRequirement) {
                    buttons.Add(travelButtons[index]);
                    index++;
                }
            }

            buttons.Add(travelButtons[4]);
            popup.SetUpPopup(travelTitleKey.key, travelMessageKey.key, timeAdvanceMessageKey.key, buttons,
                             ExecutionState.PopupPause, TravelToIsland);
        }

        /// <summary>
        /// Loads the travel scene and sets up the island travel routine.
        /// </summary>
        private void TravelToIsland(int buttonIndex) {
            if(buttonIndex >= 4) {
                FindObjectOfType<PlayerStatsUI>().ShowHideCanvas(true);
                FindObjectOfType<PortSceneTransition>().startedTransition = false;
                return;
            }
            
            GameMaster.Instance.AdvanceOneTimePeriod();
            
            GameMaster.Instance.SelectedIslandSize = (IslandSizes) Mathf.Min(buttonIndex, 2);
            GameMaster.Instance.PlayerStatsBeforeIsland = GameMaster.Instance.PlayerStats;

            var components = GameObject.FindGameObjectsWithTag(playerComponentsTag);
            foreach(var component in components) {
                if(!component.GetComponent<PlayerStatsUI>()) Destroy(component);
            }

            var player = FindObjectOfType<PlayerController>();
            player.UpdateGameMasterPlayerStats();
            Destroy(player.gameObject);
            
            anim.SetTrigger(DepartToIsland);
            shipCamera.m_Priority = 12;
            SceneManager.LoadSceneAsync(travelSceneIndex, LoadSceneMode.Additive);
        }

        /// <summary>
        /// Unloads the town scene and changes active directional light.
        /// </summary>
        public void UnloadTown() {
            FindObjectOfType<PlayerStatsUI>().ShowHideCanvas(false);
            FindObjectOfType<ShipTravelLoadingController>().StartLoadingSequence(islandsSceneIndex);
        }

        /// <summary>
        /// Loads the islands scenes.
        /// </summary>
        public void LoadIsland() {
            anim.SetTrigger(ArriveAtIsland);
            
            IslandLoader = FindObjectOfType<IslandLevelLoader>();
            IslandLoader.LoadIslands();
        }

        /// <summary>
        /// Unlocks the player and stops the travel routine.
        /// </summary>
        public void AtIslandArrival() {
            FindObjectOfType<PlayerSpawnPositionBasedOnLastScene>().UnlockPlayer();
            shipCamera.m_Priority = 9;
            IslandLoader.DisplayIslandType();
        }
        
        #endregion

        #region Travel to Town
        
        /// <summary>
        /// Starts the travel routine to the town.
        /// </summary>
        public void StartTravelToTown() {
            var popup = Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();
            var buttons = new List<CanvasPopupDialog.ButtonSettings>();
            FindObjectOfType<PlayerStatsUI>().ShowHideCanvas(false);
            
            buttons.Add(travelButtons[3]);
            buttons.Add(travelButtons[4]);
            
            popup.SetUpPopup(travelTitleKey.key, backToTownMessageKey.key, buttons,
                              ExecutionState.PopupPause, TravelToTown);
        }
        
        /// <summary>
        /// Loads the travel scene and sets up the town travel routine.
        /// </summary>
        private void TravelToTown(int buttonIndex) {
            if(buttonIndex >= 4) {
                FindObjectOfType<PlayerStatsUI>().ShowHideCanvas(true);
                FindObjectOfType<IslandsSceneTransition>().startedTransition = false;
                return;
            }

            var components = GameObject.FindGameObjectsWithTag(playerComponentsTag);
            foreach(var component in components) {
                Destroy(component);
            }
            
            var player = FindObjectOfType<PlayerController>();
            player.UpdateGameMasterPlayerStats();
            Destroy(player.gameObject);
            
            anim.SetTrigger(DepartToTown);
            shipCamera.m_Priority = 12;
            SceneManager.LoadScene(travelSceneIndex, LoadSceneMode.Additive);
        }
        
        /// <summary>
        /// Unloads the island scene and changes active directional light.
        /// </summary>
        public void UnloadIsland() {
            FindObjectOfType<ShipTravelLoadingController>().StartLoadingSequence(townSceneIndex);
        }

        /// <summary>
        /// Loads the town scene.
        /// </summary>
        public void LoadTown() {
            GameMaster.Instance.SpawnInFrontOfStore = false;
            
            anim.SetTrigger(ArriveAtTown);
        }
        
        /// <summary>
        /// Unlocks the player and stops the travel routine.
        /// </summary>
        public void AtTownArrival() {
            FindObjectOfType<PlayerSpawnPositionBasedOnLastScene>().UnlockPlayer(true);
            shipCamera.m_Priority = 9;
            SceneManager.UnloadSceneAsync(travelSceneIndex);
        }
        
        #endregion
    }
}