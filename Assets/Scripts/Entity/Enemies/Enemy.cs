using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Entity.Enemies {
    /// <summary>
    /// Base enemy class.
    /// </summary>
    public class Enemy : Entity {
        [Header("Player Settings")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private LayerMask playerLayer = 10;

        [Header("Enemy Settings")]
        [SerializeField] protected EnemySettings settings;
        private float movementSpeed = 1f;
        private int primaryAttackDamage = 1;
        private int secondaryAttackDamage = 1;
        private EnemyType enemyType;
        protected GameObject player;
        protected IDamageable playerDamageable;

        protected virtual void Start() {
            player = GameObject.FindWithTag("Player");
            playerDamageable = player.GetComponent<IDamageable>();
            SetEnemyValues();
        }

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
        }

        protected virtual void FixedUpdate() {
            Move();
        }

        protected virtual void Move() {
            agent.SetDestination(player.transform.position);
            if(Vector3.Distance(transform.position, player.transform.position) < 1f) {
                Attack();
            }
        }

        protected virtual void Attack() {
            playerDamageable.Damage(primaryAttackDamage);
        }
    }
}