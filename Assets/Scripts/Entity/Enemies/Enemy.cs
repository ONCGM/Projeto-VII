using System;
using System.Collections;
using System.Collections.Generic;
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
        [Header("Player ItemSettings")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private LayerMask playerLayer = 10;

        [Header("Enemy ItemSettings")]
        [SerializeField] protected EnemySettings settings;
        private float movementSpeed = 1f;
        private int primaryAttackDamage = 1;
        private int secondaryAttackDamage = 1;
        private EnemyType enemyType;

        [Header("Loot ItemSettings")] 
        [SerializeField, Range(0f, 1f)] private float chanceToDrop = 0.1f;
        [SerializeField] private List<ItemSettings> itemsToDrop = new List<ItemSettings>();
        
        protected GameObject player;
        protected IDamageable playerDamageable;
        [SerializeField] private GameObject DamageCanvasPrefab;

        protected virtual void Start() {
            player = GameObject.FindWithTag("Player");
            playerDamageable = player.GetComponent<IDamageable>();
            SetEnemyValues();
        }

        /// <summary>
        /// Does the initial configuration of an enemy entity.
        /// </summary>
        protected virtual void SetEnemyValues() {
            Health = settings.baseMaxHealth;
            maxHealth = settings.baseMaxHealth;
            Stamina = settings.baseMaxStamina;
            maxStamina = settings.baseMaxStamina;
            maxLevel = settings.maxLevelCap;
            movementSpeed = settings.baseMoveSpeed;
            primaryAttackDamage = settings.baseDamagePrimaryAttack;
            secondaryAttackDamage = settings.baseDamageSecondaryAttack;
            enemyType = settings.enemyType;
            chanceToDrop = settings.chanceToDropItem;
            itemsToDrop = settings.itemsToDrop;
        }

        // Calls other methods that need to be updated constantly.
        protected virtual void FixedUpdate() {
            Move();
        }

        /// <summary>
        /// Moves the enemy entity. Uses a NavMeshAgent.
        /// </summary>
        protected virtual void Move() {
            agent.SetDestination(player.transform.position);
            if(Vector3.Distance(transform.position, player.transform.position) < 1f) {
                // TODO: Attack through collision.
                Attack();
            }
        }

        /// <summary>
        /// Attacks, will damage anything that is in front of the entity and in range.
        /// </summary>
        protected virtual void Attack() {
            // TODO: Extend damage to play animations, detect collision and damage other entities.
            playerDamageable.Damage(primaryAttackDamage);
            Instantiate(DamageCanvasPrefab, transform.position, Quaternion.identity)
                .GetComponent<DamageCanvas>().damageValue = primaryAttackDamage;
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