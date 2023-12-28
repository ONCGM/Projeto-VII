using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Entity.Player {
    /// <summary>
    /// Holds player data and used for serialization.
    /// </summary>
    [Serializable]
    public struct PlayerStats {
        public int Health {
            get => health;
            set => health = value;
        }

        public int MaxHealth {
            get => maxHealth;
            set => maxHealth = value;
        }

        public int Stamina {
            get => stamina;
            set => stamina = value;
        }

        public int MaxStamina {
            get => maxStamina;
            set => maxStamina = value;
        }

        public int MeleeDamage {
            get => meleeDamage;
            set => meleeDamage = value;
        }

        public int RangedDamage {
            get => rangedDamage;
            set => rangedDamage = value;
        }

        public int MovementSpeed {
            get => movementSpeed;
            set => movementSpeed = value;
        }
        
        public int Level {
            get => level;
            set => level = Mathf.Clamp(value, 0, 30);
        }

        public int Experience {
            get => experience;
            set => experience = value;
        }

        public int TotalExperience {
            get => totalExperience;
            set => totalExperience = value;
        }

        public int Coins {
            get => coins;
            set => coins = value;
        }

        public int CurrentUpgradeLevel {
            get => currentUpgradeLevel;
            set => currentUpgradeLevel = Mathf.Clamp(value, 0, 7);
        }

        public List<InventoryItemEntry> CurrentInventory {
            get => currentInventory;
            set => currentInventory = value;
        }

        public int InventorySize {
            get => inventorySize;
            set => inventorySize = value;
        }

        [SerializeField]
        private int health, maxHealth, stamina, maxStamina,
                    meleeDamage, rangedDamage, movementSpeed,
                    level, experience, totalExperience, coins,
                    currentUpgradeLevel, inventorySize;
        private List<InventoryItemEntry> currentInventory;

        public PlayerStats(int health, int maxHealth, int stamina, 
                           int melee, int ranged, int movement,
                           int maxStamina, int lvl, int experience, int totalExperience,
                           int coins, List<InventoryItemEntry> currentInventory,
                           int inventorySize, int currentUpgradeLvl) : this() {
            Health = health;
            MaxHealth = maxHealth;
            Stamina = stamina;
            MaxStamina = maxStamina;
            MeleeDamage = melee;
            RangedDamage = ranged;
            MovementSpeed = movement;
            Level = lvl;
            Experience = experience;
            TotalExperience = totalExperience;
            Coins = coins;
            CurrentInventory = currentInventory;
            InventorySize = inventorySize;
            CurrentUpgradeLevel = currentUpgradeLvl;
        }
    }
}