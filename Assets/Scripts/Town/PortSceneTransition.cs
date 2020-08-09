using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Town {
    public class PortSceneTransition : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Scene Loading")] [SerializeField]
        private int sceneIndex;

        [Header("Transition")] private bool startedTransition;
        #pragma warning restore 0649
        
        private void OnTriggerEnter(Collider other) {
            if(!other.CompareTag("Player") || startedTransition) return;
            SceneManager.LoadScene(sceneIndex);
            startedTransition = true;
        }
    }
}