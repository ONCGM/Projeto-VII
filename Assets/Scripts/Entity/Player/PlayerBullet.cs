using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Enemies;
using UnityEngine;

namespace Entity.Player {
    /// <summary>
    /// Fired by the player special attack, deals damage with enemies it encounters through collisions.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class PlayerBullet : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Damage Settings")] 
        [SerializeField] private AnimationCurve damageLossOverDistance;

        [Header("Projectile Settings")] 
        [SerializeField] private float bulletInitialForce = 35f;

        [SerializeField] private int timeUntilSelfDestruction = 15;

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
        
        private Vector3 initialPosition = Vector3.zero;

        public Vector3 InitialPosition {
            get => initialPosition;
            set => initialPosition = value;
        }

        private Rigidbody rb;
        #pragma warning restore 0649
        
        
        private void Awake() {
            rb = GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * bulletInitialForce, ForceMode.Impulse);
            Destroy(gameObject, timeUntilSelfDestruction);
        }

        private void LateUpdate() {
            CheckRange();
        }

        private void CheckRange() {
            if((initialPosition - transform.position).magnitude > maxRange) {
                rb.constraints = RigidbodyConstraints.None;
            }
        }

        private void OnCollisionEnter(Collision other) {
            if(!other.gameObject.CompareTag("Enemy")) {
                Destroy(gameObject);
                return;
            }
            
            var damageAmount = Mathf.RoundToInt(damage *
                                                (damageLossOverDistance.Evaluate(Mathf.InverseLerp(0f, maxRange,
                                                                                                   (initialPosition -
                                                                                                    transform.position)
                                                                                                   .magnitude))));
            other.gameObject.GetComponent<Enemy>().Damage(damageAmount);
            Debug.Log($"Hit {other.gameObject.name} for {damageAmount} damage.");
            Destroy(gameObject);
        }
    }
}