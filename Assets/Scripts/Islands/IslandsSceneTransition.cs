using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Islands {
    public class IslandsSceneTransition : MonoBehaviour {
        [Header("Scene Loading")] [SerializeField]
        private int sceneIndex;

        [Header("Transition")] private bool startedTransition;

        private void OnTriggerEnter(Collider other) {
            if(!other.CompareTag("Player") || startedTransition) return;
            SceneManager.LoadScene(sceneIndex);
            startedTransition = true;
        }
    }
}