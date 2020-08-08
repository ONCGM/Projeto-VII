using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game {
    public class LoadGameScene : MonoBehaviour {
        [Header("Scene to Load")]
        [SerializeField] private int sceneIndex;

        private void Start() {
            SceneManager.LoadScene(sceneIndex);
        }
    }
}