using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class StoreTransition : MonoBehaviour {
    public int sceneIndex;
    private Animator anim;

    private void Awake() {
        anim = GetComponent<Animator>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        UniversalAdditionalCameraData cameraData = GameObject.FindWithTag("MainCamera").GetComponent<Camera>().GetUniversalAdditionalCameraData();
        cameraData.cameraStack.Clear();
        cameraData.cameraStack.Add(gameObject.GetComponent<Camera>());
        anim.SetTrigger("Open");
    }

    public void ChangeScene() {
        SceneManager.LoadScene(sceneIndex);
    }

    public void SelfDestruct() {
        Destroy(gameObject);
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameObject.FindWithTag("MainCamera").GetComponent<Camera>().GetUniversalAdditionalCameraData().cameraStack.Clear();
    }
}
