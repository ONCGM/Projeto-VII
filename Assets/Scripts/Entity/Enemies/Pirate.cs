using System.Collections;
using System.Collections.Generic;
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
        private bool inRoutine;
        
        #pragma warning restore 0649
        
        protected override void SetEnemyValues() {
            base.SetEnemyValues();
            rangedDamagePerBullet = settings.baseDamagePrimaryAttack;
        }

        protected override IEnumerator MoveTowardsEntity() {
            agent.SetDestination(targetEntity.transform.position);
            if(Vector3.Distance(transform.position, targetEntity.transform.position) < 1f) {
                Attack();
            } else if(Vector3.Distance(transform.position, targetEntity.transform.position) > 8f){
                if(Random.value < 0.01f && !inRoutine) StartCoroutine(nameof(AttackRanged));
            }
            
            yield break;
        }

        /// <summary>
        /// Ranged attack coroutine. Allow the enemy to use a weapon a long ranges.
        /// </summary>
        private IEnumerator AttackRanged() {
            inRoutine = true;
            agent.speed = settings.baseMoveSpeed * 0.025f;
            yield return new WaitForSeconds(1f);
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
            inRoutine = false;
        }
    }
}