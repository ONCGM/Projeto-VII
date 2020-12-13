using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Entity.Player;
using Game;
using UI.Localization;
using UI.Popups;
using UnityEngine;
using UnityEngine.AI;
using static UI.Popups.CanvasPopupDialog;

namespace Store {
    /// <summary>
    /// Allows the player to change floors.
    /// </summary>
    public class FloorController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")]
        [SerializeField] private bool displayPopupToChangeFirstFloor;
        [SerializeField] private bool displayPopupToChangeSecondFloor;
        [SerializeField] public int currentFloor = 0;
        
        [Header("Cameras")]
        [SerializeField] private CinemachineVirtualCamera secondFloorCamera;

        [Header("Teleport Points")] 
        [SerializeField] private Transform firstFloorTeleportPosition;
        [SerializeField] private Transform secondFloorTeleportPosition;

        [Header("Localization Keys")] 
        [SerializeField] private LocalizedString titleKey;
        [SerializeField] private LocalizedString messageKey, 
                                                 confirmKey,
                                                 cancelKey;
        
        [Header("Prefabs")] 
        [SerializeField] private GameObject popupPrefab;
        
        // Variable Jungle.
        private PlayerController player;
        private CanvasPopupDialog popupDialog;
        private List<ButtonSettings> travelButtons = new List<CanvasPopupDialog.ButtonSettings> ();
        
        // Actions.
        /// <summary>
        /// Triggers the travel between floors if invoked.
        /// </summary>
        public static Action OnTriggerTravel;
        
        #pragma warning restore 0649

        // Sets up the class.
        private void Awake() {
            player = FindObjectOfType<PlayerController>();
            travelButtons.Add(new ButtonSettings(confirmKey.key, PopupButtonHighlight.Normal, 0));
            travelButtons.Add(new ButtonSettings(cancelKey.key, PopupButtonHighlight.Highlight, 1));
            OnTriggerTravel += StartTeleportToNextFloor;
        }
        
        /// <summary>
        /// Opens a popup to teleport the player to the other floor.
        /// </summary>
        private void StartTeleportToNextFloor() {
            if(displayPopupToChangeFirstFloor && displayPopupToChangeSecondFloor) {
                DisplayPopupToChangeFloors();
            } else {
                switch(currentFloor) {
                    case 0 when !displayPopupToChangeFirstFloor:
                        TeleportPlayerToNextFloor();
                        break;
                    case 0 when displayPopupToChangeFirstFloor:
                        DisplayPopupToChangeFloors();
                        break;
                    default: {
                        if(currentFloor > 0 && !displayPopupToChangeSecondFloor) TeleportPlayerToNextFloor();
                        else if(currentFloor > 0 && displayPopupToChangeSecondFloor) DisplayPopupToChangeFloors();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Opens the popup to change floors.
        /// </summary>
        private void DisplayPopupToChangeFloors() {
            if(popupDialog != null) return;

            popupDialog = Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();

            popupDialog.SetUpPopup(titleKey.key, messageKey.key, travelButtons, ExecutionState.PopupPause, i => {
                if(i <= 0) TeleportPlayerToNextFloor();
                else FloorTravelTrigger.OnResetTriggerLock?.Invoke();
            });
        }
        
        /// <summary>
        /// Teleports the player to the other floor.
        /// </summary>
        private void TeleportPlayerToNextFloor() {
            var store = FindObjectOfType<StoreController>();
            store.HideStoreUi();
            if(store.StoreOpen) return;
            
            if(currentFloor == 0) {
                // Go to floor 1
                player.GetComponent<NavMeshAgent>().enabled = false;
                player.CanMove = false;
                player.transform.position = secondFloorTeleportPosition.position;
                player.transform.rotation = secondFloorTeleportPosition.rotation;
                secondFloorCamera.enabled = true;
                currentFloor++;
            } else {
                // Go back to floor 0
                player.GetComponent<NavMeshAgent>().enabled = false;
                player.CanMove = false;
                player.transform.position = firstFloorTeleportPosition.position;
                player.transform.rotation = firstFloorTeleportPosition.rotation;
                secondFloorCamera.enabled = false;
                currentFloor = 0;
                FindObjectOfType<BedController>().canTriggerSleep = true;
            }

            player.CanMove = true;
            player.GetComponent<NavMeshAgent>().enabled = true;
            FloorTravelTrigger.OnResetTriggerLock?.Invoke();
        }
        
        // Clears the action.
        private void OnDestroy() {
            OnTriggerTravel = null;
        }
    }
}