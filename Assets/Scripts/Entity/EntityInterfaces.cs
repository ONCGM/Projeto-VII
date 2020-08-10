using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entity {
    public interface IEntity {
        /// <summary>
        /// The health of this entity.
        /// </summary>
        int Health { get; set; }

        /// <summary>
        /// The stamina of this entity.
        /// </summary>
        int Stamina { get; set; }

        /// <summary>
        /// The level of this entity.
        /// </summary>
        int Level { get; set; }
        
        /// <summary>
        /// The experience of this entity.
        /// </summary>
        int Experience { get; set; }
    }
    
    public interface IDamageable
    {
        /// <summary>
        /// Deal damage to this entity.
        /// </summary>
        /// <param name="amount"> Amount of damage to inflict.</param>
        void Damage(int amount);

        /// <summary>
        /// Heal this entity.
        /// </summary>
        /// <param name="amount"> Amount of healing.</param>
        void Heal(int amount);

        /// <summary>
        /// Kill this entity.
        /// </summary>
        void Kill();
    }

    public interface IInteractable {
        void StartInteraction();

        void EndInteraction();
    }
}