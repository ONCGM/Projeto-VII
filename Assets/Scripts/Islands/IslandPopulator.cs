using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Islands {
    /// <summary>
    /// Populates islands with enemies and loot.
    /// </summary>
    public class IslandPopulator : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Enemies settings")]
        [SerializeField] private bool spawnEnemies;
        [SerializeField] private List<Transform> enemySpawnPositions = new List<Transform>();
        [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();

        [Header("Loot Settings")] 
        [SerializeField] private bool spawnLootBox;
        [SerializeField] private List<Transform> lootSpawnPositions = new List<Transform>();
        [SerializeField] private List<GameObject> lootPrefabs = new List<GameObject>();
        
        #pragma warning restore 0649
        
        /// <summary>
        /// Calls the spawning methods.
        /// </summary>
        private void Awake() {
            PopulateEnemies();
            PopulateLoot();
        }
       
        /// <summary>
        /// Adds enemies to the island based on current player level.
        /// </summary>
        private void PopulateEnemies() {
        
        }
        
        /// <summary>
        /// Adds loots to the island based on current player level.
        /// </summary>
        private void PopulateLoot() {
        
        }
    }
}