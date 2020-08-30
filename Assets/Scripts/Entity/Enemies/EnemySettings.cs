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
        [SerializeField] public int baseDamageSecondaryAttack;
        [SerializeField] public int maxLevelCap;
        [SerializeField] public EnemyType enemyType;

        [Header("Loot Values")] 
        [SerializeField, Range(0f, 1f)] public float chanceToDropItem;
        [SerializeField] public List<ItemSettings> itemsToDrop = new List<ItemSettings>();
        
        [Header("AI ItemSettings")]
        [SerializeField] public bool isAggressive;
        [SerializeField] public bool attacksEntitiesWhoDamagedThisEntity;
        [SerializeField] public bool shouldPatrolWhenIdle;
        [SerializeField, Range(1f, 100f)] public float spottingRange;
    }
}