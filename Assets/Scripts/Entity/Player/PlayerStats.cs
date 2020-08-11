using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entity.Player {
    /// <summary>
    /// Holds player data and used for serialization.
    /// </summary>
    [Serializable]
    public struct PlayerStats {
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Stamina { get; set; }
        public int MaxStamina { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Coins { get; set; }
        public List<object> CurrentInventory { get; set; }
        public List<int> CurrentUpgrades { get; set; }

        public PlayerStats(int health, int maxHealth, int stamina, 
                           int maxStamina, int level, int experience,
                           int coins, List<object> currentInventory,
                           List<int> currentUpgrades) {
            Health = health;
            MaxHealth = maxHealth;
            Stamina = stamina;
            MaxStamina = maxStamina;
            Level = level;
            Experience = experience;
            Coins = coins;
            CurrentInventory = currentInventory;
            CurrentUpgrades = currentUpgrades;
        }
    }
}