using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Islands {
    /// <summary>
    /// Populates islands with enemies and loot.
    /// </summary>
    public class IslandPopulator : MonoBehaviour {
        [Header("Enemies settings")]
        [SerializeField] private bool spawnEnemies;
        [SerializeField] private List<Transform> enemySpawnPositions = new List<Transform>();
        [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();

        [Header("Loot Settings")] 
        [SerializeField] private bool spawnLootBox;
        [SerializeField] private List<Transform> lootSpawnPositions = new List<Transform>();
        [SerializeField] private List<GameObject> lootPrefabs = new List<GameObject>();

        private void Awake() {
            
        }
    }
}