using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Game {
    /// <summary>
    /// Controls the directional lights of a scene and updates it
    /// to match with the game master current time of day.
    /// </summary>
    public class DirectionalLightController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")]
        [SerializeField, Range(0.1f, 30f)] private float transitionTimeBetweenPositions = 5f;
        [SerializeField] private bool enableOnAwake = false;
        [SerializeField] private LightScene scene = LightScene.Town;
        
        // Fields & Properties.
        private readonly List<Light> directionalLights = new List<Light>();
        private Light mainLight;

        /// <summary>
        /// Which scene is this light on.
        /// </summary>
        public LightScene Scene {
            get => scene;
            set => scene = value;
        }

        // Enums.
        /// <summary>
        /// Defines from which scene a light is from.
        /// </summary>
        public enum LightScene {
            Town,
            Travel,
            Island,
            Store
        }
        #pragma warning restore 0649

        /// <summary>
        /// Sets up the lights and updates the main light position.
        /// </summary>
        private void Awake() {
            for(var i = 0; i < transform.childCount; i++) {
                var component = transform.GetChild(i).GetComponent<Light>();
                if(component != null) directionalLights.Add(component);
            }

            foreach(var directionalLight in directionalLights) {
                directionalLight.enabled = false;
            }
            
            mainLight = directionalLights[0];
            UpdateLightToCurrentTime(true);
            mainLight.enabled = enableOnAwake;
            GameMaster.OnTimeOfDayUpdated += UpdateLightToCurrentTime;
        }

        /// <summary>
        /// Updates the light direction to reflect the time of day.
        /// </summary>
        public void UpdateLightToCurrentTime() {
            mainLight.transform.DOLocalRotate(GetLightRotation().eulerAngles, transitionTimeBetweenPositions);
            mainLight.DOColor(GetLightColor(), transitionTimeBetweenPositions);
            mainLight.DOIntensity(GetLightIntensity(), transitionTimeBetweenPositions);
        }

        /// <summary>
        /// Updates the light direction to reflect the time of day.
        /// </summary>
        /// <param name="immediately"> Animate the change or do it immediately.
        /// False animates, true is instantaneous.</param>
        public void UpdateLightToCurrentTime(bool immediately) {
            if(immediately) {
                mainLight.transform.rotation = GetLightRotation();
                mainLight.color = GetLightColor();
                mainLight.intensity = GetLightIntensity();
            } else {
                mainLight.transform.DOLocalRotate(GetLightRotation().eulerAngles, transitionTimeBetweenPositions);
                mainLight.DOColor(GetLightColor(), transitionTimeBetweenPositions);
                mainLight.DOIntensity(GetLightIntensity(), transitionTimeBetweenPositions);
            }
        }
        
        /// <summary>
        /// Return the next rotation to use when transitioning game time.
        /// </summary>
        private Quaternion GetLightRotation() => directionalLights[1 + (int) GameMaster.Instance.CurrentTimeOfDay].transform.rotation;
        
        /// <summary>
        /// Return the next color to use when transitioning game time.
        /// </summary>
        private Color GetLightColor() => directionalLights[1 + (int) GameMaster.Instance.CurrentTimeOfDay].color;
        
        /// <summary>
        /// Return the next intensity to use when transitioning game time.
        /// </summary>
        private float GetLightIntensity() => directionalLights[1 + (int) GameMaster.Instance.CurrentTimeOfDay].intensity;

        /// <summary>
        /// Enables the light that this controller
        /// has authority over.
        /// </summary>
        public void EnableLights() {
            mainLight.enabled = true;
        }

        /// <summary>
        /// Disables all the lights that this controller
        /// has authority over.
        /// </summary>
        public void DisableLights() {
            foreach(var directionalLight in directionalLights) {
                directionalLight.enabled = false;
            }
        }

        // Unsubscribes from game master events.
        private void OnDestroy() {
            GameMaster.OnTimeOfDayUpdated -= UpdateLightToCurrentTime;
        }
    }
}