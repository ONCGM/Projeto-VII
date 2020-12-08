using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using Entity.Player;
using Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Menu {
    /// <summary>
    /// Ends the game on the 10th day.
    /// </summary>
    public class EndGameLauncher : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")] 
        [SerializeField] private int endGameSceneIndex = 11;
        
        #pragma warning restore 0649
        
        // Setup.
        private void Awake() {
            GameMaster.OnEndGameDayUpdate += LaunchEndGameSequence;
        }

        // De-setup.
        private void OnDestroy() {
            GameMaster.OnEndGameDayUpdate -= LaunchEndGameSequence;
        }

        /// <summary>
        /// Displays the end game screen.
        /// </summary>
        private void LaunchEndGameSequence() {
            SceneManager.LoadSceneAsync(endGameSceneIndex, LoadSceneMode.Additive);

            for(var i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                if(i == endGameSceneIndex) continue;
                if(SceneManager.GetSceneByBuildIndex(i).isLoaded) SceneManager.UnloadSceneAsync(i);
            }

            if(FindObjectOfType<PlayerController>()) FindObjectOfType<PlayerController>().CanMoveOverride = false;
            if(FindObjectOfType<GameDebugHelper>()) FindObjectOfType<GameDebugHelper>().DebugEnabled = false;
            if(FindObjectOfType<GameDebugHelper>()) FindObjectOfType<GameDebugHelper>().DebugEnabled = false;
            if(FindObjectOfType<PlayerStatsUI>()) FindObjectOfType<PlayerStatsUI>().ShowHideCanvas(false);
            if(FindObjectOfType<StoreMusicController>()) FindObjectOfType<StoreMusicController>().TriggerMusicStop();
            if(FindObjectOfType<IslandMusicController>()) FindObjectOfType<IslandMusicController>().TriggerMusicStop();
            if(FindObjectOfType<TownMusicController>()) FindObjectOfType<TownMusicController>().TriggerMusicStop();
            if(FindObjectOfType<WavesAmbienceController>()) FindObjectOfType<WavesAmbienceController>().TriggerEventStop();
            if(FindObjectOfType<WavesAmbienceController>()) FindObjectOfType<WavesAmbienceController>().TriggerEventStop();
            if(FindObjectOfType<WavesAmbienceController>()) FindObjectOfType<WavesAmbienceController>().TriggerEventStop();
        }
    }
}