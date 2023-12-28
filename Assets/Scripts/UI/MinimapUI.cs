using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Entity.Player;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace UI {
    /// <summary>
    /// Controls, displays and hides minimap UI.
    /// </summary>
    public class MinimapUI : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Scene Indexes")] 
        [SerializeField] private int islandsBuildIndex = 3;
        [SerializeField] private int townBuildIndex = 5;
        
        [Header("Settings")]
        [SerializeField, Range(0f, 3f)] private float animationSpeed = 0.5f;
        
        // Components.
        private CanvasGroup canvasGroup;
        private Camera minimapCamera;
        private Transform player;
        private bool updateMinimap = true;

        #pragma warning restore 0649

        // Sets up references and the minimap if the scene should use it.
        private void Start() {
            canvasGroup = GetComponent<CanvasGroup>();
            minimapCamera = GetComponentInChildren<Camera>();
            player = FindObjectOfType<PlayerController>().transform;

            if(SceneManager.GetSceneByBuildIndex(islandsBuildIndex).isLoaded ||
               SceneManager.GetSceneByBuildIndex(townBuildIndex).isLoaded) {
                DOVirtual.Float(canvasGroup.alpha, 1f, animationSpeed, value => canvasGroup.alpha = value);
            } else {
                updateMinimap = false;
                DOVirtual.Float(canvasGroup.alpha, 0f, animationSpeed, value => canvasGroup.alpha = value);
            }
        }

        /// <summary>
        /// Call the update in the camera position.
        /// </summary>
        private void LateUpdate() {
            if(updateMinimap && player != null) CameraFollowPlayer();
        }

        /// <summary>
        /// Sets the camera position to be on top of the player position.
        /// </summary>
        private void CameraFollowPlayer() {
            var position = new Vector3(player.transform.position.x, 125f, player.transform.position.z);
            
            minimapCamera.transform.position = position;
        }
    }
}