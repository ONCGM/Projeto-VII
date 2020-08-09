using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ship {
    public class ShipTravelRoutine : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Scene Index")] 
        [SerializeField] private int townSceneIndex;
        [SerializeField] private int islandsSceneIndex;
        
        #pragma warning restore 0649
        
        private void Update() {
            if(Input.GetKeyDown(KeyCode.Alpha1)) {
                SceneManager.LoadScene(islandsSceneIndex);
            }

            if(Input.GetKeyDown(KeyCode.Alpha2)) {
                SceneManager.LoadScene(townSceneIndex);
            }
        }
    }
}