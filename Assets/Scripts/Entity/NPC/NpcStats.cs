using System.Collections;
using System.Collections.Generic;
using Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entity.NPC {
    /// <summary>
    /// Holds the settings of the game npcs and their behaviour.
    /// </summary>
    [CreateAssetMenu(fileName = "NPC_Settings_", menuName = "Scriptable Objects/NPC")]
    public class NpcStats : ScriptableObject {
        [Header("Base Values")]
        [SerializeField] public int amountOfCoins;
        
        [Header("Multipliers")] 
        [SerializeField, Range(1f, 2f)] public float basicStatsMultiplier;
        [SerializeField, Range(0, 10)] public int levelVariationBasedOnPlayerLevel;
        
        [Header("AI Settings")]
        [FormerlySerializedAs("howManyItemsToViewBeforeBuying")]
        [SerializeField, Range(1, 10)] public int howManyItemsToBrowse;
        [SerializeField, Range(0f, 1f)] public float chanceToBuySomething;
        
        [Header("References")]
        [SerializeField] public List<GameObject> npcsVariationsToUse = new List<GameObject>();
    }
}