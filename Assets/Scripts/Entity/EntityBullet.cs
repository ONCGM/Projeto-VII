using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Enemies;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Entity {
    /// <summary>
    /// Fired by the player special attack or enemies, deals damage with entities it encounters through collisions.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class EntityBullet : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Damage Settings")] 
        [SerializeField] private bool useTagToDamage = true;
        [SerializeField] private string tagToDamage = string.Empty;
        [SerializeField] private AnimationCurve damageLossOverDistance;

        [Header("Projectile Settings")] 
        [SerializeField] private float bulletInitialForce = 35f;

        [SerializeField] private int timeUntilSelfDestruction = 15;

        [Header("Damage Canvas Prefab")] 
        [SerializeField] private GameObject DamageCanvasPrefab;
        

        private int damage = 15;
        public int Damage {
            get => damage;
            set => damage = value;
        }
        
        private float maxRange = 50;
        public float MaxRange {
            get => maxRange;
            set => maxRange = value;
        }

        private Entity bulletOwner;

        public Entity BulletOwner {
            get => bulletOwner;
            set => bulletOwner = value;
        }

        private Vector3 initialPosition = Vector3.zero;

        public Vector3 InitialPosition {
            get => initialPosition;
            set => initialPosition = value;
        }

        private Rigidbody rb;
        #pragma warning restore 0649
        
        /// <summary>
        /// Sets up the class.
        /// </summary>
        private void Awake() {
            rb = GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * bulletInitialForce, ForceMode.Impulse);
            Destroy(gameObject, timeUntilSelfDestruction);
        }

        /// <summary>
        /// Updates methods.
        /// </summary>
        private void LateUpdate() {
            CheckRange();
        }

        /// <summary>
        /// Check for traveled distance.
        /// </summary>
        private void CheckRange() {
            if((initialPosition - transform.position).magnitude > maxRange) {
                rb.constraints = RigidbodyConstraints.None;
            }
        }

        // Check collisions.
        private void OnCollisionEnter(Collision other) {
            if(useTagToDamage) {
                if(!other.gameObject.CompareTag(tagToDamage)) {
                    Destroy(gameObject);
                    return;
                }
            } else {
                if(!other.gameObject.GetComponent<Entity>() || other.gameObject.GetComponent<Entity>().Equals(bulletOwner)) {
                    Destroy(gameObject);
                    return;
                }
            }

            var damageAmount = Mathf.RoundToInt(damage *
                                                (damageLossOverDistance.Evaluate(Mathf.InverseLerp(0f, maxRange,
                                                        (initialPosition -
                                                         transform.position)
                                                        .magnitude))));
            other.gameObject.GetComponent<IDamageable>().Damage(damageAmount, BulletOwner);

            Instantiate(DamageCanvasPrefab, transform.position, Quaternion.identity).GetComponent<DamageCanvas>().damageValue = damageAmount;

            Destroy(gameObject);
        }
    }
}