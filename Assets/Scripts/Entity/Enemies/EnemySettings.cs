    using System.Collections;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Entity.Enemies {
    /// <summary>
    /// Holds the settings of the game enemies and their behaviour.
    /// </summary>
    [CreateAssetMenu(fileName = "Enemy_Settings_", menuName = "Scriptable Objects/Enemy")]
    public class EnemySettings : ScriptableObject {
        [Header("Base Values")]
        [SerializeField] public int baseMaxHealth;
        [SerializeField] public int baseMaxStamina;
        [SerializeField, Range(0f, 25f)] public float baseMoveSpeed;
        [SerializeField] public int baseDamagePrimaryAttack;
        [SerializeField] public int attackingRange;
        [SerializeField] public int maxLevelCap;
        [SerializeField] public EnemyType enemyType;

        [Header("Multipliers")] 
        [SerializeField, Range(1f, 2f)] public float basicStatsMultiplier;
        [SerializeField, Range(1f, 2f)] public float attackStatsMultiplier;
        [SerializeField, Range(1f, 2f)] public float eliteStatsMultiplier;
        [SerializeField, Range(0, 10)] public int levelVariationBasedOnPlayerLevel;
        
        [Header("Loot Values")] 
        [SerializeField, Range(0f, 1f)] public float chanceToDropItem;
        [SerializeField] public List<ItemSettings> itemsToDrop = new List<ItemSettings>();
        
        [Header("AI Settings")]
        [SerializeField] public bool isAggressive;
        [SerializeField] public bool attacksEntitiesWhoDamagedThisEntity;
        [SerializeField] public bool patrolBackAndForth;
        [SerializeField, Range(1f, 20f)] public float patrollingRange;
        [SerializeField, Range(1f, 100f)] public float spottingRange;

        [Header("Elite Settings")] 
        [SerializeField] public int spawnEliteAfterLevel;
        [SerializeField] public float chanceToSpawnAsElite;
    }
}