using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entity.Player;
using Game;
using Items;
using UnityEngine;
using UnityEngine.AI;
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
        [SerializeField, Range(0.01f, 1f)] protected float targetPositionUpdateInterval = 0.2f;
        [SerializeField, Range(0.2f, 5f)] protected float patrollingPlayerSearchFrequency = 2f;
        [SerializeField, Range(0.05f, 3f)] protected float patrollingTargetPositionTolerance = 0.3f;

        [Header("Loot Settings")] 
        [SerializeField, Range(0f, 1f)] protected float chanceToDrop = 0.1f;
        [SerializeField] protected List<ItemSettings> itemsToDrop = new List<ItemSettings>();
        
        [Header("Other")]
        [SerializeField] protected GameObject DamageCanvasPrefab;
        
        
        // Movement related field and variables.
        protected Entity targetEntity;
        protected Vector3 lastPatrolOffset;
        protected bool isPatrolling;
        protected bool isChasing;
        protected bool isAttacking;
        protected AiState currentState;

        protected enum AiState {
            Patrolling,
            Chasing,
            Attacking,
            Idle
        }
        
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

            lastPatrolOffset = (settings.patrolBackAndForth ? (Random.value > 0.5f ? Vector3.forward : Vector3.right) * settings.patrollingRange :
                                new Vector3(Random.Range(-settings.patrollingRange, settings.patrollingRange), 
                                            0f, Random.Range(-settings.patrollingRange, settings.patrollingRange)));
            
            StartCoroutine(settings.isAggressive ? nameof(MoveTowardsEntity) : nameof(PatrolArea));
        }
        
        /// <summary>
        /// Moves the enemy entity. Uses a NavMeshAgent.
        /// </summary>
        protected virtual IEnumerator MoveTowardsEntity() {
            currentState = AiState.Chasing;
            isChasing = true;
            isAttacking = false;
            isPatrolling = false;
            
            var waitForFrames = new WaitForSeconds(targetPositionUpdateInterval);
            
            if(targetEntity == null) {
                currentState = AiState.Idle;
                isChasing = false;
                SearchForPlayer();
                yield break;
            }

            while(Vector3.Distance(transform.position, targetEntity.transform.position) > settings.attackingRange) {
                agent.SetDestination(targetEntity.transform.position);
                yield return waitForFrames;

                if(Vector3.Distance(transform.position, targetEntity.transform.position) < settings.spottingRange) continue;
                targetEntity = null;
                currentState = AiState.Idle;
                isChasing = false;
                StartCoroutine(nameof(PatrolArea));
                yield break;
            }
            
            Attack();
        }
        
        /// <summary>
        /// Moves around in a certain pattern if not aggro.
        /// </summary>
        protected virtual IEnumerator PatrolArea() {
            currentState = AiState.Patrolling;
            isChasing = false;
            isAttacking = false;
            isPatrolling = true;
            
            var waitForFrames = new WaitForSeconds(patrollingPlayerSearchFrequency);

            agent.SetPath(GeneratePatrolPath());
            
            while(agent.remainingDistance > patrollingTargetPositionTolerance) {
                SearchForPlayer();
                yield return waitForFrames;
            }

            currentState = AiState.Idle;
            isPatrolling = false;
            SearchForPlayer();
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos() {
            Gizmos.color = targetEntity == null ? Color.red : Color.green;
            
            if(!Application.isPlaying) return;
            if(!agent.hasPath) return;
            
            for(int i = 0; i < agent.path.corners.Length - 1; i++) {
                Gizmos.DrawLine(agent.path.corners[i], agent.path.corners[i + 1]);
            }
            
            Gizmos.color = Color.blue;
            
            Gizmos.DrawWireSphere(transform.position, settings.attackingRange);
        }
        #endif
        
        /// <summary>
        /// Chooses a new patrolling path and checks if it is valid.
        /// If it can't find a valid path, will default to returning a path
        /// towards the nearest edge in the navigation mesh.
        /// </summary>
        protected virtual NavMeshPath GeneratePatrolPath() {
            var path = new NavMeshPath();
            
            for(var i = (int) settings.patrollingRange; i > 1; i--) {
                var patrolOffset = (settings.patrolBackAndForth ? 
                                        (lastPatrolOffset.normalized * -1) * i :
                                        new Vector3(Random.Range(-i, i), 0f, Random.Range(-i, i)));
            
                NavMesh.CalculatePath(transform.position, transform.position + patrolOffset, NavMesh.AllAreas, path);

                if(path.status != NavMeshPathStatus.PathInvalid) {
                    lastPatrolOffset = patrolOffset;
                    return path;
                }
            }

            agent.FindClosestEdge(out var hit);
            NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path);

            return path;
        }
        
        /// <summary>
        /// Check if the player is in range of the enemy.
        /// </summary>
        protected virtual void SearchForPlayer() {
            if(!settings.isAggressive) {
                if(!isPatrolling) StartCoroutine(nameof(PatrolArea));
                return;
            }
            
            var results = new Collider[]{};
            Physics.OverlapSphereNonAlloc(transform.position, settings.spottingRange, results, playerLayer.value);

            foreach(var result in results) {
                if(result.gameObject.GetComponent<PlayerController>() == null) continue;
                targetEntity = result.gameObject.GetComponent<PlayerController>();
                if(!isChasing) StartCoroutine(nameof(MoveTowardsEntity));
                return;
            }
            
            targetEntity = null;
            if(!isPatrolling) StartCoroutine(nameof(PatrolArea));
        }
        
        /// <summary>
        /// Attacks, will damage anything that is in front of the entity and in range.
        /// </summary>
        protected virtual void Attack() {
            currentState = AiState.Attacking;
            isChasing = false;
            isAttacking = true;
            isPatrolling = false;
            StopAllCoroutines();
            
            // TODO: Extend damage to play animations, detect collision and damage other entities.
            Instantiate(DamageCanvasPrefab, transform.position, Quaternion.identity)
                .GetComponent<DamageCanvas>().damageValue = primaryAttackDamage;

            StartCoroutine(nameof(MoveTowardsEntity));
        }

        // Updated to enable infighting between enemies.
        public override void Damage(int amount, Entity dealer) {
            if(settings.attacksEntitiesWhoDamagedThisEntity) { targetEntity = dealer; } 
            else if(dealer.gameObject.CompareTag(playerTag)) { targetEntity = dealer; }

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
            var availableItems = itemsToDrop.Where(itemSettings => GameMaster.Instance.PlayerStats.Level > itemSettings.minimumPlayerLevelToSpawn).ToList();
            var selectedItem = availableItems[Random.Range(0, availableItems.Count)];
            
            Instantiate(selectedItem.itemPrefab, transform.position, Quaternion.identity).
                GetComponent<ItemDrop>().SetItemBasedOnSettings(selectedItem);
        }
    }
}