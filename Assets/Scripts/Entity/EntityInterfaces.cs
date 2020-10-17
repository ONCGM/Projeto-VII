using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entity {
    /// <summary>
    /// Base interface for entities accessing base values.
    /// </summary>
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
        /// The level cap of this entity.
        /// </summary>
        int MaxLevel { get; set; }
        
        /// <summary>
        /// The experience of this entity.
        /// </summary>
        int Experience { get; set; }

        /// <summary>
        /// Amount of coins that the entity has.
        /// </summary>
        int Coins { get; set; }
    }
    
    /// <summary>
    /// Base interface for entities dealing and receiving damage.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Deal damage to this entity.
        /// </summary>
        /// <param name="amount"> Amount of damage to inflict. </param>
        /// <param name="dealer"> Entity who dealt the damage. </param>
        void Damage(int amount, Entity dealer);

        /// <summary>
        /// Heal this entity.
        /// </summary>
        /// <param name="amount"> Amount of healing. </param>
        void Heal(int amount);

        /// <summary>
        /// Kill this entity.
        /// </summary>
        void Kill();
    }

    /// <summary>
    /// Base interface for the player and entities interacting.
    /// </summary>
    public interface IInteractable {
        void Interact();
    }
}