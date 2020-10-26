using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entity.Player;
using Game;
using Items;
using UI;
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
        
        // Collider.
        protected SphereCollider attackingCollider;
        
        // Anims.
        protected static readonly int AttackAnim = Animator.StringToHash("Attack");
        protected static readonly int DamagedAnim = Animator.StringToHash("Damaged");
        protected static readonly int DeadAnim = Animator.StringToHash("Dead");
        protected static readonly int SpeedAnim = Animator.StringToHash("Speed");
        
        // UI.
        protected EnemyHealthBarUI hpBarUI;

        protected enum AiState {
            Patrolling,
            Chasing,
            Attacking,
            Idle
        }
        
        #pragma warning restore 0649

        // Unity Events.
        protected virtual void Start() {
            attackingCollider = GetComponentInChildren<SphereCollider>();
            hpBarUI = GetComponentInChildren<EnemyHealthBarUI>();
            GameMaster.OnGameExecutionStateUpdated += GameStateUpdated;
            SetEnemyValues();
            InvokeRepeating(nameof(RecoverStamina), 1f, 1f);
        }

        /// <summary>
        /// Recovers the enemy stamina.
        /// </summary>
        protected virtual void RecoverStamina() {
            Stamina = Mathf.Clamp(Stamina + 2, 0, MaxStamina);
        }

        protected virtual void Update() {
            anim.SetFloat(SpeedAnim, agent.velocity.magnitude);
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
            
            MaxHealth = Mathf.CeilToInt(Health * (isElite ? settings.eliteStatsMultiplier : 1f) * GameMaster.Instance.GameDifficulty);
            Health = MaxHealth;
            MaxStamina = Mathf.CeilToInt(MaxStamina * (isElite ? settings.eliteStatsMultiplier : 1f) * GameMaster.Instance.GameDifficulty);
            Stamina = MaxStamina;
            primaryAttackDamage = Mathf.CeilToInt(primaryAttackDamage * (isElite ? settings.eliteStatsMultiplier : 1f) * GameMaster.Instance.GameDifficulty);
            
            MaxLevel = settings.maxLevelCap;
            movementSpeed = settings.baseMoveSpeed;
            enemyType = settings.enemyType;
            chanceToDrop = settings.chanceToDropItem  * GameMaster.Instance.GameDifficultyReversed;
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
            
            if(agent != null && agent.isOnNavMesh) agent.SetDestination(targetEntity.transform.position);
            
            while(Vector3.Distance(transform.position, targetEntity.transform.position) > settings.attackingRange) {
                if(agent != null && agent.isOnNavMesh) agent.SetDestination(targetEntity.transform.position);
                yield return waitForFrames;

                if(Vector3.Distance(transform.position, targetEntity.transform.position) < settings.spottingRange) continue;
                targetEntity = null;
                currentState = AiState.Idle;
                isChasing = false;
                StartCoroutine(nameof(PatrolArea));
                yield break;
            }
            
            anim.SetTrigger(AttackAnim);
            currentState = AiState.Attacking;
            isChasing = false;
            isAttacking = true;
            isPatrolling = false;
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

            if(agent.isOnNavMesh) {
                agent.SetPath(GeneratePatrolPath());
            } else {
                yield break;
            }
            
            while(agent.isOnNavMesh && agent.remainingDistance > patrollingTargetPositionTolerance) {
                SearchForPlayer();
                yield return waitForFrames;
            }

            currentState = AiState.Idle;
            isPatrolling = false;
            SearchForPlayer();
        }

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
            if(!agent.isOnNavMesh) return;
            
            if(!settings.isAggressive) {
                if(!isPatrolling) StartCoroutine(nameof(PatrolArea));
                return;
            }
            
            // ReSharper disable once Unity.PreferNonAllocApi
            var results = Physics.OverlapSphere(transform.position, 
                                                settings.spottingRange, playerLayer);

            foreach(var result in results) {
                if(result.gameObject.GetComponent<PlayerController>() == null) continue;
                targetEntity = result.gameObject.GetComponent<PlayerController>();
                StopAllCoroutines();
                StartCoroutine(nameof(MoveTowardsEntity));
                return;
            }
            
            targetEntity = null;
            if(!isPatrolling) StartCoroutine(nameof(PatrolArea));
        }
        
        /// <summary>
        /// Attacks, will damage anything that is in front of the entity and in range.
        /// </summary>
        public virtual void Attack() {
            if(Stamina < 3) return;
            Stamina -= 3;
            StopAllCoroutines();

            if(CheckAttackCollision(attackingCollider)) {
                Instantiate(DamageCanvasPrefab, transform.position, Quaternion.identity)
                    .GetComponent<DamageCanvas>().damageValue = primaryAttackDamage;
            }

            isAttacking = false;
            currentState = AiState.Idle;
            StartCoroutine(nameof(MoveTowardsEntity));
        }
        
        /// <summary>
        /// Test collisions for melee hits.
        /// </summary>
        /// <param name="referenceSphere"> Weapon sphere collider. </param>
        protected virtual bool CheckAttackCollision(SphereCollider referenceSphere) {
            // ReSharper disable once Unity.PreferNonAllocApi
            var contacts = Physics.OverlapSphere(referenceSphere.transform.position, referenceSphere.radius, 
                                               Physics.AllLayers, QueryTriggerInteraction.Collide);
            
            foreach(var contact in contacts) {
                if(!contact.gameObject.GetComponent<Entity>()) continue;
                if(contact.gameObject.Equals(gameObject)) continue;
                contact.gameObject.GetComponent<Entity>().Damage(primaryAttackDamage, this);
                return true;
            }

            return false;
        }

        // Updated to enable infighting between enemies.
        public override void Damage(int amount, Entity dealer) {
            anim.SetTrigger(DamagedAnim);
            Health = Mathf.Max(Health - amount, 0);
            
            if(Health <= 0) {
                Kill();
                if(dealer.CompareTag(playerTag)) 
                    dealer.GetComponent<PlayerController>().AddExperience(AmountOfExperienceToDrop());
            }

            hpBarUI.UpdateUI();

            if(!settings.attacksEntitiesWhoDamagedThisEntity && !dealer.gameObject.CompareTag(playerTag)) return;
            targetEntity = dealer;
            StopAllCoroutines();
            StartCoroutine(nameof(MoveTowardsEntity));
        }

        public override void Kill() {
            StopAllCoroutines();
            
            anim.SetTrigger(DeadAnim);

            foreach(var coll in GetComponents<Collider>()) {
                coll.enabled = false;
            }

            agent.enabled = false;
            
            hpBarUI.FadeOutAnimation();
            
            GenerateDrop();
            
            // ReSharper disable once DelegateSubtraction
            GameMaster.OnGameExecutionStateUpdated -= GameStateUpdated;
        }

        /// <summary>
        /// Chooses and spawns a drop based on player progression and random chance.
        /// </summary>
        protected virtual void GenerateDrop() {
            // TODO: Set Coins based on economy.
            if(Random.value > settings.chanceToDropItem) return;
            var availableItems = itemsToDrop.Where(itemSettings =>
                                                       GameMaster.Instance.PlayerStats.Level >
                                                       itemSettings.minimumPlayerLevelToSpawn).ToList();

            if(availableItems.Count < 1) {
                FindObjectOfType<PlayerController>()
                    .AddCoins(Mathf.RoundToInt(Random.Range(1, 50) * GameMaster.Instance.GameDifficultyReversed));
                return;
            }

            var selectedItem = availableItems[Random.Range(0, availableItems.Count)];

            FindObjectOfType<PlayerController>()
                .AddCoins(Mathf.RoundToInt(Random.Range(1, 25) * GameMaster.Instance.GameDifficultyReversed));
            if(selectedItem == null) return;
                Instantiate(selectedItem.itemPrefab, transform.position, Quaternion.identity).GetComponent<ItemDrop>()
                    .SetItemBasedOnSettings(selectedItem);
        }
        
        
        /// <summary>
        /// Returns the experience amount to give to the player or killing entity.
        /// </summary>
        protected virtual int AmountOfExperienceToDrop() {
            return Mathf.FloorToInt((MaxHealth + MaxStamina + 
                                    primaryAttackDamage + movementSpeed +
                                    (settings.spottingRange * 0.5f) / 5f) * 
                                    GameMaster.Instance.GameDifficultyReversed);
        }

        /// <summary>
        /// Follows the behaviour of the game state if it is changed.
        /// </summary>
        protected virtual void GameStateUpdated(ExecutionState state) {
            switch(state) {
                case ExecutionState.Normal:
                    StartCoroutine(settings.isAggressive ? nameof(MoveTowardsEntity) : nameof(PatrolArea));
                    break;
                case ExecutionState.PopupPause:
                    StopAllCoroutines();
                    break;
                case ExecutionState.FullPause:
                    StopAllCoroutines();
                    break;
            }
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
    }
}