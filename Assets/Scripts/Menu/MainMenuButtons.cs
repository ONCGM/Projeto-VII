using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu {
    public class MainMenuButtons : MonoBehaviour {
        [Header("ItemSettings")] 
        [SerializeField] private int gameSceneIndex = 2;
        
        public void StartGameScene() {
            SceneManager.LoadScene(gameSceneIndex);
        }
        
        public void QuitToDesktop() {
            Application.Quit();
        }
    }
}