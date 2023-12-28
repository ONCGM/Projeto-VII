using System.Collections;
using System.Collections.Generic;
using Entity.Player;
using Game;
using Items;
using UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Town {
    /// <summary>
    /// Positions the player in the correct spawn position based on last scene.
    /// </summary>
    public class PlayerSpawnPositionBasedOnLastScene : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings and components")]
        [SerializeField] private PlayerController player;
        [SerializeField] public Transform portSpawnPosition, storeSpawnPosition;
        
        // Components and variables.
        private NavMeshAgent agent;
        private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
        private List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
        
        #pragma warning restore 0649
        
        /// <summary>
        /// Sets the correct player position.
        /// </summary>
        private void Awake() {
            agent = player.GetComponent<NavMeshAgent>();
            agent.enabled = false;
            player.CanMove = false;
            player.IsInsideShop = false;
            
            meshRenderers.AddRange(player.GetComponentsInChildren<MeshRenderer>());
            foreach(var meshRenderer in meshRenderers) {
                meshRenderer.enabled = false;
            }
            
            skinnedMeshRenderers.AddRange(player.GetComponentsInChildren<SkinnedMeshRenderer>());
            foreach(var meshRenderer in skinnedMeshRenderers) {
                meshRenderer.enabled = false;
            }

            player.transform.position = GameMaster.Instance.SpawnInFrontOfStore ? storeSpawnPosition.position : portSpawnPosition.position;
        }

        /// <summary>
        /// Unlocks the player controller and enables his graphics and UI.
        /// </summary>
        public void UnlockPlayer(bool updateSpawnPosition = false, bool toggleUI = true) {
            if(updateSpawnPosition) {
                player.transform.position = GameMaster.Instance.SpawnInFrontOfStore
                                                ? storeSpawnPosition.position
                                                : portSpawnPosition.position;
            }

            if(toggleUI) {
                FindObjectOfType<PlayerStatsUI>().ShowHideCanvas(true);
            }

            agent.enabled = true;
            player.CanMove = true;
            player.IsInsideShop = false;
            
            foreach(var meshRenderer in meshRenderers) {
                meshRenderer.enabled = true;
            }
            
            skinnedMeshRenderers.AddRange(player.GetComponentsInChildren<SkinnedMeshRenderer>());
            foreach(var meshRenderer in skinnedMeshRenderers) {
                meshRenderer.enabled = true;
            }
            
            player.PlayerIslandInventory = new Inventory(player.Inventory.InventorySize, new List<InventoryItemEntry>());
        }
    }
}