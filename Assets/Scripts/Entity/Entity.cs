using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Entity {
    /// <summary>
    /// Base class for all entities in the game.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(CapsuleCollider))]
    public abstract class Entity : MonoBehaviour, IEntity, IDamageable {
        // Basic stats.
        [SerializeField] private int health = 10;
        [SerializeField] private int maxHealth = 10;
        public virtual int Health {
            get => health;
            set => health = value;
        }
        
        [SerializeField] private int stamina = 10;
        [SerializeField] private int maxStamina = 10;
        public virtual int Stamina {
            get => stamina;
            set => stamina = value;
        }

        [SerializeField] private int level = 0;
        public virtual int Level {
            get => level;
            set => level = value;
        }

        [SerializeField] private int experience = 0;
        public virtual int Experience {
            get => experience;
            set => experience = value;
        }
        
        // Basic Components.
        protected NavMeshAgent agent;
        protected Animator anim;
        protected CapsuleCollider capsuleCollider;

        /// <summary>
        /// Base Awake, initializing the generic components.
        /// </summary>
        protected virtual void Awake() {
            agent = GetComponent<NavMeshAgent>();
            anim = GetComponent<Animator>();
            capsuleCollider = GetComponent<CapsuleCollider>();
        }

        // General methods.
        public virtual void Damage(int amount) {
            Health -= amount;
            if(Health <= 0) Kill();
        }

        public virtual void Heal(int amount) {
            Health += amount;
            if(Health > maxHealth) Health = maxHealth;
        }

        public virtual void Kill() {
            Destroy(gameObject);
        }
    }
}