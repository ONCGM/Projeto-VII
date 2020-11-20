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
        private IslandLevelLoader islandLoader;

        #pragma warning restore 0649

        // Sets the ship up.
        private void Awake() {
            anim = GetComponent<Animator>();
            shipCamera = GetComponentInChildren<CinemachineVirtualCamera>();
            DontDestroyOnLoad(gameObject);
            upgradeSettings = Resources.Load<PlayerUpgradeSettings>(playerSettingsPath);
            
            // Toggle light if in town scene.
            if(SceneManager.GetActiveScene().buildIndex == townSceneIndex) {
                var lightController = FindObjectOfType<DirectionalLightController>();
                lightController.UpdateLightToCurrentTime(true);
                lightController.EnableLights();
            }
            
            
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
            var townLight = GameObject.FindGameObjectWithTag(townLightTag).GetComponent<DirectionalLightController>();
            var seaLight = GameObject.FindGameObjectWithTag(seaLightTag).GetComponent<DirectionalLightController>();

            townLight.DisableLights();
            
            seaLight.UpdateLightToCurrentTime();
            seaLight.EnableLights();
            
            SceneManager.UnloadSceneAsync(townSceneIndex);

            StartCoroutine(nameof(LoadIsland));
        }

        /// <summary>
        /// Loads the island scene.
        /// </summary>
        private IEnumerator LoadIsland() {
            var async = SceneManager.LoadSceneAsync(islandsSceneIndex, LoadSceneMode.Additive);
            var waitAFrame = new WaitForEndOfFrame();

            while(!async.isDone) {
                yield return waitAFrame;
            }
            
            var seaLight = GameObject.FindGameObjectWithTag(seaLightTag).GetComponent<DirectionalLightController>();;
            var islandLight = GameObject.FindGameObjectWithTag(islandLightTag).GetComponent<DirectionalLightController>();;
            
            seaLight.DisableLights();
            
            islandLight.UpdateLightToCurrentTime();
            islandLight.EnableLights();

            anim.SetTrigger(ArriveAtIsland);
            
            islandLoader = FindObjectOfType<IslandLevelLoader>();
            islandLoader.LoadIslands();
        }

        /// <summary>
        /// Unlocks the player and stops the travel routine.
        /// </summary>
        public void AtIslandArrival() {
            FindObjectOfType<PlayerSpawnPositionBasedOnLastScene>().UnlockPlayer();
            shipCamera.m_Priority = 9;
            SceneManager.UnloadSceneAsync(travelSceneIndex);
            islandLoader.DisplayIslandType();
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
            var islandLight = GameObject.FindGameObjectWithTag(islandLightTag).GetComponent<DirectionalLightController>();
            var seaLight = GameObject.FindGameObjectWithTag(seaLightTag).GetComponent<DirectionalLightController>();
            
            islandLight.DisableLights();
            seaLight.UpdateLightToCurrentTime();
            seaLight.EnableLights();

            islandLoader.UnloadIslands();
            islandLoader = null;

            SceneManager.UnloadSceneAsync(islandsSceneIndex);

            StartCoroutine(nameof(LoadTown));
        }

        /// <summary>
        /// Loads the town scene.
        /// </summary>
        private IEnumerator LoadTown() {
            var async = SceneManager.LoadSceneAsync(townSceneIndex, LoadSceneMode.Additive);
            var waitAFrame = new WaitForEndOfFrame();

            while(!async.isDone) {
                yield return waitAFrame;
            }

            var seaLight = GameObject.FindGameObjectWithTag(seaLightTag).GetComponent<DirectionalLightController>();
            var townLight = GameObject.FindGameObjectWithTag(townLightTag).GetComponent<DirectionalLightController>();
            
            seaLight.DisableLights();
            
            townLight.UpdateLightToCurrentTime();
            townLight.EnableLights();
            
            
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