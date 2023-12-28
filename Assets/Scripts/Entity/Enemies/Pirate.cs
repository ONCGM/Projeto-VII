using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

namespace Entity.Enemies {
    /// <summary>
    /// Pirate Enemy class. The average joe in difficulty.
    /// </summary>
    public class Pirate : Enemy {
        #pragma warning disable 0649
        [Header("Ranged Attacks")]
        private int rangedDamagePerBullet = 15;
        private int rangedBulletsPerAttack = 1;
        [SerializeField, Range(0f, 90f)] private float bulletSpreadAngle = 70f; 
        [SerializeField, Range(2f, 55f)] private float rangedAttackMaxRange = 50f;
        [SerializeField] private Transform bulletSpawnPosition;
        [SerializeField] private GameObject bulletPrefab;
        [Header("Extra Sounds")]
        [SerializeField, EventRef] private string hurtEvent;
        
        #pragma warning restore 0649
        
        // Adds extra values needed for this enemy.
        protected override void SetEnemyValues() {
            base.SetEnemyValues();
            rangedDamagePerBullet = settings.baseDamagePrimaryAttack;
        }

        protected override IEnumerator MoveTowardsEntity() {
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

            if(agent != null && agent.isOnNavMesh && targetEntity != null) {
                var difference = targetEntity.transform.position - transform.position;
                agent.SetDestination(targetEntity.transform.position - difference.normalized);
            }
            
            while(targetEntity != null && Vector3.Distance(transform.position, targetEntity.transform.position) > settings.attackingRange) {
                if(agent != null && agent.isOnNavMesh) {
                    var difference = targetEntity.transform.position - transform.position;
                    agent.SetDestination(targetEntity.transform.position - difference.normalized);
                }
                
                yield return waitForFrames;

                if(targetEntity != null && Vector3.Distance(transform.position, targetEntity.transform.position) < settings.spottingRange) continue;
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
        
        // Overrides base attack to instead attack with a ranged projectile.
        public override void Attack() {
            if(Stamina < 5 || IsDead) return;
            Stamina -= 5;
            StartCoroutine(nameof(AttackRanged));
            PlaySfx(attackEvent);
        }

        /// <summary>
        /// Ranged attack coroutine. Allow the enemy to use a weapon a long ranges.
        /// </summary>
        private IEnumerator AttackRanged() {
            agent.speed = settings.baseMoveSpeed * 0.02f;

            yield return new WaitUntil(() => anim.GetCurrentAnimatorClipInfo(0)[0].clip.GetHashCode().Equals(AttackAnim));
            
            var angleOffsetPerBullet = bulletSpreadAngle / rangedBulletsPerAttack;
            var position = bulletSpawnPosition.position;

            for(var i = 0; i < rangedBulletsPerAttack; i++) {
                var rotation = bulletSpawnPosition.rotation.eulerAngles;
                
                if(rangedBulletsPerAttack > 1) {
                    rotation.y -= bulletSpreadAngle * 0.5f;
                    rotation.y += angleOffsetPerBullet * i;
                }

                var bullet = Instantiate(bulletPrefab, position, Quaternion.Euler(rotation)).GetComponent<EntityBullet>();
                bullet.Damage = rangedDamagePerBullet;
                bullet.MaxRange = rangedAttackMaxRange;
                bullet.InitialPosition = position;
                bullet.BulletOwner = this;
            }
            
            // ReSharper disable once Unity.InefficientPropertyAccess
            agent.speed = settings.baseMoveSpeed;
            isAttacking = false;
            currentState = AiState.Chasing;

            StartCoroutine(nameof(MoveTowardsEntity));
        }

        // Ads sfx.
        public override void Damage(int amount, Entity dealer) {
            base.Damage(amount, dealer);
            PlaySfx(hurtEvent);
        }
    }
}