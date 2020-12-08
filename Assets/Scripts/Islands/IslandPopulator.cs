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
        [SerializeField, Range(0.01f, 1f)] private float chanceOfSpawningLoot = 0.4f;

        [Header("Player Settings")] 
        [SerializeField, Range(1, 45)] private int maxLevel = 30;
        
        // Other.
        private int spawnedEnemyCount;
        
        #pragma warning restore 0649
        
        /// <summary>
        /// Calls the spawning methods.
        /// </summary>
        private void Start() {
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
                GameObject enemy;
                
                switch(GameMaster.Instance.CurrentIslandType) {
                    case IslandType.BrawlIsland:
                        enemy = (stats.Level > spawnPirateEnemiesAfterLevel ?
                                     (Random.value < chanceOfSpawningPirate ? pirateEnemyPrefab : critterEnemyPrefab)
                                     : critterEnemyPrefab);
                        break;
                    case IslandType.TreasureIsland:
                        if(Random.value > chanceToSpawn) { continue; }
                        enemy = (stats.Level > spawnPirateEnemiesAfterLevel ?
                                         (Random.value <  Mathf.Min(chanceOfSpawningPirate * 2f, 1f) ? pirateEnemyPrefab : critterEnemyPrefab)
                                         : critterEnemyPrefab);
                        break;
                    case IslandType.MerchantIsland:
                        if(Random.value > chanceToSpawn) { continue; }
                        enemy = (stats.Level > spawnPirateEnemiesAfterLevel ?
                                         (Random.value < Mathf.Max(chanceOfSpawningPirate * 0.5f, 0.1f) ? pirateEnemyPrefab : critterEnemyPrefab)
                                         : critterEnemyPrefab);
                        break;
                    default:
                        enemy = (stats.Level > spawnPirateEnemiesAfterLevel ?
                                     (Random.value < chanceOfSpawningPirate ? pirateEnemyPrefab : critterEnemyPrefab)
                                     : critterEnemyPrefab);
                        break;
                }

                Instantiate(enemy, spawnPos.position, spawnPos.rotation);
                spawnedEnemyCount++;
            }
        }

        /// <summary>
        /// Adds loots to the islands.
        /// </summary>
        private void PopulateLoot() {
            if(!spawnLootBox) { return; }

            if(GameMaster.Instance.CurrentIslandType == IslandType.TreasureIsland) {
                foreach(var spawn in lootSpawnPositions) {
                    Instantiate(lootPrefabs[Random.Range(0, lootPrefabs.Count)], spawn.position, Quaternion.Euler(Vector3.zero));
                }
            } else {
                foreach(var spawn in lootSpawnPositions) {
                    if(Random.value > chanceOfSpawningLoot) continue;
                    Instantiate(lootPrefabs[Random.Range(0, lootPrefabs.Count)], spawn.position, Quaternion.Euler(Vector3.zero));
                }
            }
        }
    }
}