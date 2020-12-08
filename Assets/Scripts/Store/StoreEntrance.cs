using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using Entity.Player;
using UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Store {
    /// <summary>
    /// Triggers the transition between town and store scenes.
    /// </summary>
    public class StoreEntrance : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Scene Loading")] 
        [SerializeField] private int sceneIndex;
        [SerializeField] private bool isStore = false;

        [Header("Transition")] [SerializeField]
        private Camera mainCamera;

        [SerializeField] private GameObject transitionPrefab;
        private bool startedTransition;

        #pragma warning restore 0649
        
        // Set player speed;
        private void Awake() {
            FindObjectOfType<PlayerController>().IsInsideShop = true;
        }

        // Collision.
        private void OnTriggerEnter(Collider other) {
            if(!other.CompareTag("Player") || startedTransition) return;
            FindObjectOfType<PlayerStatsUI>().ShowHideCanvas(false);
            var player = FindObjectOfType<PlayerController>();
            player.CanMove = false;
            player.UpdateGameMasterPlayerStats();
            
            if(isStore) {
                FindObjectOfType<StoreMusicController>().TriggerMusicStop();
            } else {
                FindObjectOfType<TownMusicController>().TriggerMusicStop();
            }
            
            GameObject transitionCamera = Instantiate(transitionPrefab);
            DontDestroyOnLoad(transitionCamera);
            transitionCamera.GetComponent<StoreTransition>().sceneIndex = sceneIndex;
            mainCamera.GetUniversalAdditionalCameraData().cameraStack.Clear();
            mainCamera.GetUniversalAdditionalCameraData().cameraStack.Add(transitionCamera.GetComponent<Camera>());
            startedTransition = true;
        }
    }
}