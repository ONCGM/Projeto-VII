using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using UnityEngine;

namespace Town {
    /// <summary>
    /// Animates the lighthouse when night. 
    /// </summary>
    public class TownLighthouse : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")]
        [SerializeField] private TimeOfDay timeToTurnOn = TimeOfDay.Night;
        [SerializeField, Range(1f, 10f)] private float animationSpeed = 3f;
        [SerializeField, Range(5f, 35f)] private float lightAngle = 15f;

        // Components.
        private Light lighthouseLight;
        #pragma warning restore 0649

        // Sets the class up.
        private void Awake() {
            lighthouseLight = GetComponentInChildren<Light>();
            EnableLightBasedOnTime();
            GameMaster.OnTimeOfDayUpdated += EnableLightBasedOnTime;
        }

        // Unsubscribes the class from game master.
        private void OnDestroy() {
            GameMaster.OnTimeOfDayUpdated -= EnableLightBasedOnTime;
            DOTween.Kill(gameObject);
        }

        /// <summary>
        /// Enables the light based on the game time.
        /// </summary>
        private void EnableLightBasedOnTime() {
            lighthouseLight.enabled = GameMaster.Instance.CurrentTimeOfDay == timeToTurnOn;
            if(lighthouseLight.enabled) AnimateLight();
            else DOTween.Kill(gameObject);
        }

        /// <summary>
        /// Animates the light with DOTween.
        /// </summary>
        private void AnimateLight() =>
            lighthouseLight.transform.DORotate(new Vector3(lightAngle, 360f, 0f), animationSpeed, RotateMode.FastBeyond360).
                            OnComplete(AnimateLight).SetEase(Ease.Linear).id = gameObject;
    }
}