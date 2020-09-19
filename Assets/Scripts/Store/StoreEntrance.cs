using System;
using System.Collections;
using System.Collections.Generic;
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
        [Header("Scene Loading")] [SerializeField]
        private int sceneIndex;

        [Header("Transition")] [SerializeField]
        private Camera mainCamera;

        [SerializeField] private GameObject transitionPrefab;
        private bool startedTransition;

        #pragma warning restore 0649
        private void OnTriggerEnter(Collider other) {
            if(!other.CompareTag("Player") || startedTransition) return;
            FindObjectOfType<PlayerStatsUI>().ShowHideCanvas(false);
            FindObjectOfType<PlayerController>().CanMove = false;
            GameObject transitionCamera = Instantiate(transitionPrefab);
            DontDestroyOnLoad(transitionCamera);
            transitionCamera.GetComponent<StoreTransition>().sceneIndex = sceneIndex;
            mainCamera.GetUniversalAdditionalCameraData().cameraStack.Clear();
            mainCamera.GetUniversalAdditionalCameraData().cameraStack.Add(transitionCamera.GetComponent<Camera>());
            startedTransition = true;
        }
    }
}