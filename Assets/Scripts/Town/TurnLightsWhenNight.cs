using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

namespace Town {
    /// <summary>
    /// Enables or disables the lights based on game time.
    /// </summary>
    public class TurnLightsWhenNight : MonoBehaviour {
        [Header("Settings")]
        [SerializeField] private TimeOfDay timeToTurnOn = TimeOfDay.Night;
        
        // Components.
        private Light lightToEdit;
        
        // Sets the class up.
        private void Awake() {
            lightToEdit = GetComponentInChildren<Light>();
            EnableLightBasedOnTime();
            GameMaster.OnTimeOfDayUpdated += EnableLightBasedOnTime;
        }

        // Unsubscribes the class from game master.
        private void OnDestroy() {
            GameMaster.OnTimeOfDayUpdated -= EnableLightBasedOnTime;
        }

        /// <summary>
        /// Enables the light based on the game time.
        /// </summary>
        private void EnableLightBasedOnTime() => lightToEdit.enabled = GameMaster.Instance.CurrentTimeOfDay == timeToTurnOn;
    }
}