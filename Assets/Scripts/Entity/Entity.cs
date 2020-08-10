using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Entity {
    public abstract class Entity : MonoBehaviour, IEntity, IDamageable {
        // Basic stats.
        private int health = 10;
        public virtual int Health {
            get => health;
            set => health = value;
        }
        
        private int stamina = 10;
        public virtual int Stamina {
            get => stamina;
            set => stamina = value;
        }

        private int level = 0;
        public virtual int Level {
            get => level;
            set => level = value;
        }

        private int experience = 0;
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
        public virtual void Damage(int amount) { }
        
        public virtual void Heal(int amount) { }

        public virtual void Kill() { }
    }
}