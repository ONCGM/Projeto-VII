using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class StoreEntrance : MonoBehaviour {
    [Header("Scene Loading")]
    [SerializeField] private int sceneIndex;

    [Header("Transition")] 
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject transitionPrefab;
    private bool startedTransition;
    
    private void OnTriggerEnter(Collider other) {
        if(!other.CompareTag("Player") || startedTransition) return;
        SceneManager.LoadScene(sceneIndex);
        // GameObject transitionCamera = Instantiate(transitionPrefab);
        // DontDestroyOnLoad(transitionCamera);
        // transitionCamera.GetComponent<StoreTransition>().sceneIndex = sceneIndex;
        // mainCamera.GetUniversalAdditionalCameraData().cameraStack.Add(transitionCamera.GetComponent<Camera>());
        startedTransition = true;
    }
}
