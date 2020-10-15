using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Player;
using Game;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Islands {
    /// <summary>
    /// Populates islands with enemies and loot.
    /// </summary>
    public class IslandPopulator : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Enemies settings")]
        [SerializeField] private bool spawnEnemies;
        [SerializeField, Range(1, 15)] private int spawnPirateEnemiesAfterLevel = 5;
        [SerializeField, Range(0.2f, 0.6f)] private float minChanceOfSpawning;
        [SerializeField, Range(0.4f, 0.9f)] private float maxChanceOfSpawning;
        [SerializeField, Range(0.2f, 0.8f)] private float chanceOfSpawningPirate;
        [SerializeField] private List<Transform> enemySpawnPositions = new List<Transform>();
        [SerializeField] private GameObject critterEnemyPrefab;
        [SerializeField] private GameObject pirateEnemyPrefab;
        [Header("Loot Settings")] 
        [SerializeField] private bool spawnLootBox;
        [SerializeField] private List<Transform> lootSpawnPositions = new List<Transform>();
        [SerializeField] private List<GameObject> lootPrefabs = new List<GameObject>();

        [Header("Player Settings")] 
        [SerializeField, Range(1, 45)] private int maxLevel = 30;
        
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
            if(!spawnEnemies) { return; }

            var stats = GameMaster.Instance.PlayerStats;
            var chanceToSpawn = Mathf.Lerp(minChanceOfSpawning, maxChanceOfSpawning,
                                           Mathf.InverseLerp(1, maxLevel, stats.Level));

            foreach(var spawnPos in enemySpawnPositions) {
                if(Random.value > chanceToSpawn) { continue; }

                var enemy = (stats.Level > spawnPirateEnemiesAfterLevel ?
                                 (Random.value < chanceOfSpawningPirate ? pirateEnemyPrefab : critterEnemyPrefab)
                                 : critterEnemyPrefab);
                
                Instantiate(enemy, spawnPos.position, spawnPos.rotation);
            }
        }

        /// <summary>
        /// Adds loots to the island based on current player level.
        /// </summary>
        private void PopulateLoot() {
            if(!spawnLootBox) { return; }
        }
    }
}