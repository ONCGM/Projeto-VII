using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entity.Player {
    /// <summary>
    /// Holds the values for player parameters based on the upgrades the player has gotten in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "Player_Upgrade_Settings_", menuName = "Scriptable Objects/Player")]
    public class PlayerUpgradeSettings : ScriptableObject {
        [Header("Progression settings")] 
        [SerializeField, Range(1, 30)] public int baseExperienceNeededForLevelUp;
        [SerializeField, Range(1f, 3f)] public float levelUpMultiplier;
        [SerializeField, Range(1, 10)] public int upgradeEveryHowManyLevels;
        [SerializeField, Range(1f, 2f)] public float statsMultiplier;
        [SerializeField] public List<float> meleeMultiplierUpgradeValues = new List<float>();
        [SerializeField] public List<int> bulletsPerShotUpgradeValues = new List<int>();
        [SerializeField] public List<float> bulletAngleUpgradeValues = new List<float>();
        [SerializeField] public List<float> bulletRangeUpgradeValues = new List<float>();
        [SerializeField] public List<int> inventorySizeUpgradeValues = new List<int>();
        [SerializeField] public List<int> islandSizeUnlocksAtLevel = new List<int>();

        /// <summary>
        /// Returns the amount of experience the player will need for the next level.
        /// </summary>
        public int GetExperienceNeededForLevelUp(int currentPlayerLevel) {
            return Mathf.RoundToInt(baseExperienceNeededForLevelUp + Mathf.Pow(currentPlayerLevel, 2) * levelUpMultiplier);
        }

        /// <summary>
        /// Return the current player level based on total experience.
        /// </summary>
        public int GetLevelBasedOnTotalExperience(int experience) {
            return Mathf.FloorToInt(Mathf.Sqrt((experience - baseExperienceNeededForLevelUp) / levelUpMultiplier));
        }
    }
}