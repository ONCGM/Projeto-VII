using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Menu {
    /// <summary>
    /// Controls behaviour for the game's main menu.
    /// </summary>
    public class MainMenuController : MonoBehaviour {
        [Header("Cameras")] 
        [SerializeField] private CinemachineVirtualCamera mainMenuCamera; 
        [SerializeField] private CinemachineVirtualCamera loadGameCamera,
                                                          optionsMenuCamera;

        [Header("Canvasses")] 
        [SerializeField] private Canvas mainMenuCanvas; 
        [SerializeField] private Canvas loadGameCanvas, 
                                        optionsMenuCanvas;
        
        [Header("Scene Indexes")] 
        [SerializeField] private int gameSceneIndex = 2;

        private void Awake() {
            mainMenuCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            loadGameCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            optionsMenuCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        }

        public void ToOptionsMenu() {
            mainMenuCamera.enabled = false;
            mainMenuCanvas.enabled = false;
            
            loadGameCamera.enabled = false;
            loadGameCanvas.enabled = false;
            
            optionsMenuCamera.enabled = true;
            optionsMenuCanvas.enabled = true;
        }
        
        public void ToLoadGameMenu() {
            mainMenuCamera.enabled = false;
            mainMenuCanvas.enabled = false;
            
            loadGameCamera.enabled = true;
            loadGameCanvas.enabled = true;
            
            optionsMenuCamera.enabled = false;
            optionsMenuCanvas.enabled = false;
        }
        
        public void BackToMenu() {
            mainMenuCamera.enabled = true;
            mainMenuCanvas.enabled = true;
            
            loadGameCamera.enabled = false;
            optionsMenuCamera.enabled = false;
            loadGameCanvas.enabled = false;
            optionsMenuCanvas.enabled = false;
        }
        
        public void StartGameScene() {
            SceneManager.LoadScene(gameSceneIndex);
        }
        
        public void QuitToDesktop() {
            Application.Quit();
        }
    }
}