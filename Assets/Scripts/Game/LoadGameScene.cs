using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game {
    /// <summary>
    /// Insta loads menu scene on init.
    /// </summary>
    public class LoadGameScene : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Scene to Load")]
        [SerializeField] private int sceneIndex;

        #pragma warning restore 0649
        private void Start() {
            SceneManager.LoadScene(sceneIndex);
        }
    }
}