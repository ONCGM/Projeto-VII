using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Player;
using Game;
using Town;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

namespace Store {
    /// <summary>
    /// Transitions from store to town scene with a nice animation.
    /// </summary>
    public class StoreTransition : MonoBehaviour {
        public int sceneIndex;
        public string townSceneName = "Town";
        private Animator anim;
        private static readonly int Open = Animator.StringToHash("Open");

        // Unity Events.
        private void Awake() {
            anim = GetComponent<Animator>();
            SceneManager.sceneLoaded += OnSceneLoaded;
            GameMaster.Instance.SpawnInFrontOfStore = true;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            var cameraData = GameObject
                             .FindWithTag("MainCamera").GetComponent<Camera>()
                             .GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Clear();
            cameraData.cameraStack.Add(gameObject.GetComponent<Camera>());
            anim.SetTrigger(Open);
            
            if(scene.Equals(SceneManager.GetSceneByName(townSceneName))) {
                FindObjectOfType<PlayerSpawnPositionBasedOnLastScene>().UnlockPlayer(false,false);
            }
            
            FindObjectOfType<PlayerController>().LoadGameMasterPlayerStats();
        }

        /// <summary>
        /// Loads the next scene.
        /// </summary>
        public void ChangeScene() {
            SceneManager.LoadScene(sceneIndex);
        }

        /// <summary>
        /// Triggers the destruction of this object and unlocks the player controls.
        /// </summary>
        public void SelfDestruct() {
            Destroy(gameObject);
            FindObjectOfType<PlayerSpawnPositionBasedOnLastScene>()?.UnlockPlayer();
        }

        // Unsubscribes from events and clears camera stack.
        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            GameObject.FindWithTag("MainCamera").GetComponent<Camera>().GetUniversalAdditionalCameraData().cameraStack.Clear();
        }
    }
}