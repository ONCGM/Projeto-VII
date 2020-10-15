using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Player;
using Game;
using Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Entity.Enemies {
    /// <summary>
    /// Base enemy class.
    /// </summary>
    public class Enemy : Entity {
        #pragma warning disable 0649
        [Header("Player Settings")]
        [SerializeField] protected string playerTag = "Player";
        [SerializeField] protected LayerMask playerLayer = 10;

        [Header("Enemy Settings")]
        [SerializeField] protected EnemySettings settings;
        protected float movementSpeed = 1f;
        protected int primaryAttackDamage = 1;
        protected EnemyType enemyType;

        [Header("Loot Settings")] 
        [SerializeField, Range(0f, 1f)] protected float chanceToDrop = 0.1f;
        [SerializeField] protected List<ItemSettings> itemsToDrop = new List<ItemSettings>();
        
        [Header("Other")]
        [SerializeField] protected GameObject DamageCanvasPrefab;
        
        
        // Movement action.
        protected Action MoveEnemy;
        protected Entity targetEntity;
        
        #pragma warning restore 0649

        protected virtual void Start() {
            SetEnemyValues();
        }

        /// <summary>
        /// Does the initial configuration of an enemy entity.
        /// </summary>
        protected virtual void SetEnemyValues() {
            var canBeElite = GameMaster.Instance.PlayerStats.Level > settings.spawnEliteAfterLevel;
            var isElite = (Random.value < settings.chanceToSpawnAsElite) && canBeElite;
            var enemyLevel = Random.Range(GameMaster.Instance.PlayerStats.Level - settings.levelVariationBasedOnPlayerLevel, GameMaster.Instance.PlayerStats.Level + settings.levelVariationBasedOnPlayerLevel);
            
            Health = settings.baseMaxHealth;
            MaxHealth = settings.baseMaxHealth;
            Stamina = settings.baseMaxStamina;
            MaxStamina = settings.baseMaxStamina;
            primaryAttackDamage = settings.baseDamagePrimaryAttack;

            for(int i = 0; i < enemyLevel; i++) {    
                Health = Mathf.CeilToInt(Health * settings.basicStatsMultiplier);
                MaxHealth = Mathf.CeilToInt(MaxHealth * settings.basicStatsMultiplier);
                Stamina = Mathf.CeilToInt(Stamina * settings.basicStatsMultiplier);
                MaxStamina = Mathf.CeilToInt(MaxStamina * settings.basicStatsMultiplier);
                primaryAttackDamage = Mathf.CeilToInt(primaryAttackDamage * settings.attackStatsMultiplier);
            }
            
            Health = Mathf.CeilToInt(Health * (isElite ? settings.eliteStatsMultiplier : 1f));
            MaxHealth = Mathf.CeilToInt(MaxHealth * (isElite ? settings.eliteStatsMultiplier : 1f));
            Stamina = Mathf.CeilToInt(Stamina * (isElite ? settings.eliteStatsMultiplier : 1f));
            MaxStamina = Mathf.CeilToInt(MaxStamina * (isElite ? settings.eliteStatsMultiplier : 1f));
            primaryAttackDamage = Mathf.CeilToInt(primaryAttackDamage * (isElite ? settings.eliteStatsMultiplier : 1f));
            
            MaxLevel = settings.maxLevelCap;
            movementSpeed = settings.baseMoveSpeed;
            enemyType = settings.enemyType;
            chanceToDrop = settings.chanceToDropItem;
            itemsToDrop = settings.itemsToDrop;

            MoveEnemy = settings.isAggressive ? (Action) MoveTowardsEntity : PatrolArea;
        }

        // TODO Coroutine bounce for AI behaviour.
        
        /// <summary>
        /// Moves the enemy entity. Uses a NavMeshAgent.
        /// </summary>
        protected virtual void MoveTowardsEntity() {
            if(targetEntity.Equals(null)) {
                CheckForPlayer();
                return;
            }
            
            agent.SetDestination(targetEntity.transform.position);
            
            if(Vector3.Distance(transform.position, targetEntity.transform.position) < settings.attackingRange) {
                Attack();
            }
        }
        
        /// <summary>
        /// Moves around in a certain pattern if not aggro.
        /// </summary>
        protected virtual void PatrolArea() {
            // TODO patrol behaviour
            
            if(targetEntity.Equals(null) && settings.isAggressive) CheckForPlayer();
            if(!targetEntity.Equals(null)) MoveEnemy = MoveTowardsEntity;
        }

        /// <summary>
        /// Check if the player is in range of the enemy.
        /// </summary>
        protected virtual void CheckForPlayer() {
            if(!settings.isAggressive) {
                MoveEnemy = PatrolArea;
                return;
            }
            
            var results = new Collider[]{};
            Physics.OverlapSphereNonAlloc(transform.position, settings.spottingRange, results, playerLayer.value);

            foreach(var result in results) {
                if(result.gameObject.GetComponent<PlayerController>() != null) {
                    targetEntity = result.gameObject.GetComponent<PlayerController>();
                    MoveEnemy = MoveTowardsEntity;
                    return;
                }
            }
            
            targetEntity = null;
            MoveEnemy = PatrolArea;
        }
        
        /// <summary>
        /// Attacks, will damage anything that is in front of the entity and in range.
        /// </summary>
        protected virtual void Attack() {
            // TODO: Extend damage to play animations, detect collision and damage other entities.
            Instantiate(DamageCanvasPrefab, transform.position, Quaternion.identity)
                .GetComponent<DamageCanvas>().damageValue = primaryAttackDamage;
        }

        // Updated to enable infighting between enemies.
        public override void Damage(int amount, Entity dealer) {
            targetEntity = dealer;
            base.Damage(amount, dealer);
        }

        public override void Kill() {
            GenerateDrop();
            
            // TODO: Death Animation. 
            
            Destroy(gameObject);
        }

        /// <summary>
        /// Chooses and spawns a drop based on player progression and random chance.
        /// </summary>
        protected virtual void GenerateDrop() {
            if(Random.value > settings.chanceToDropItem) return;
            var selectedItem = itemsToDrop[Random.Range(0, itemsToDrop.Count)];
            
            // TODO: Check if item can spawn based on progression, if not, try again.
            
            Instantiate(selectedItem.itemPrefab, transform.position, Quaternion.identity).
                GetComponent<ItemDrop>().SetItemBasedOnSettings(selectedItem);
        }
    }
}