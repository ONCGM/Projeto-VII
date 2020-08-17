using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entity.Enemies {
    /// <summary>
    /// Pirate Enemy class. The average joe in difficulty.
    /// </summary>
    public class Pirate : Enemy {
        [Header("Ranged Attacks")]
        private int rangedDamagePerBullet = 15;
        private int rangedBulletsPerAttack = 1;
        [SerializeField, Range(0f, 90f)] private float bulletSpreadAngle = 70f; 
        [SerializeField, Range(2f, 55f)] private float rangedAttackMaxRange = 50f;
        [SerializeField] private Transform bulletSpawnPosition;
        [SerializeField] private GameObject bulletPrefab;
        private bool inRoutine;

        protected override void SetEnemyValues() {
            base.SetEnemyValues();
            rangedDamagePerBullet = settings.baseDamageSecondaryAttack;
        }

        protected override void Move() {
            agent.SetDestination(player.transform.position);
            if(Vector3.Distance(transform.position, player.transform.position) < 1f) {
                Attack();
            } else if(Vector3.Distance(transform.position, player.transform.position) > 8f){
                if(Random.value < 0.01f && !inRoutine) StartCoroutine(nameof(AttackRanged));
            }
        }

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
            }
            // ReSharper disable once Unity.InefficientPropertyAccess
            agent.speed = settings.baseMoveSpeed;
            inRoutine = false;
        }
    }
}