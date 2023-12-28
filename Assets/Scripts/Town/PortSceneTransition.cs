using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Ship;
using UnityEngine;

namespace Town {
    /// <summary>
    /// Sets up and triggers the ship travel routine.
    /// </summary>
    public class PortSceneTransition : MonoBehaviour {
        #pragma warning disable 0649

        [Header("Ship")] 
        [SerializeField] private GameObject shipPrefab;
        [SerializeField] private Transform shipSpawnPosition;
        private ShipTravelController ship;
        
        // Transition.
        public bool startedTransition;
        #pragma warning restore 0649

        // Spawns a new ship if needed.
        private void Awake() {
            if(GameMaster.Instance.ShipTravel is null) {
                GameMaster.Instance.ShipTravel = Instantiate(shipPrefab, shipSpawnPosition.position, shipSpawnPosition.rotation).GetComponent<ShipTravelController>();
            }

            ship = GameMaster.Instance.ShipTravel;
        }

        
        // Detects if the player wants to enter the ship.
        // Triggers a popup and if the player answers yes,
        // starts the ship travel sequence and de-spawns the player. 
        private void OnTriggerEnter(Collider other) {
            if(!other.CompareTag("Player") || startedTransition) return;
            ship.StartTravelToIsland();
            startedTransition = true;
        }
    }
}