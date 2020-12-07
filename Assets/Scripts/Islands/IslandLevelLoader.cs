using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using UI.Popups;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Islands {
    /// <summary>
    /// Loads and unloads the island scene level whenever the player is arriving or leaving the islands.
    /// Also triggers a text with the type of island to appear in screen when the player arrives.
    /// </summary>
    public class IslandLevelLoader : MonoBehaviour {
        #pragma warning disable 0649

        [Header("Island Text Keys")] 
        [SerializeField] private string smallIslandTextKey, mediumIslandTextKey,
            largeIslandTextKey, brawlTypeTextKey, treasureTypeTextKey, merchantTypeTextKey;
        
        [Header("Scene Indexes")] 
        [SerializeField, Range(6,8)] private int smallIslandIndex = 6;
        [SerializeField, Range(6,8)] private int mediumIslandIndex = 7;
        [SerializeField, Range(6,8)] private int largeIslandIndex = 8;

        [Header("Prefabs")]
        [SerializeField] private GameObject locationNameCanvasPrefab;
        
        /// <summary>
        /// Current type of island selected by the loader.
        /// </summary>
        public IslandType CurrentIslandType { get; private set; }

        private bool hasLoadedIslands;
        
        #pragma warning restore 0649
        
        /// <summary>
        /// Loads an island scene based on GameMaster settings.
        /// </summary>
        public void LoadIslands() {
            if(hasLoadedIslands) return;
            var islandSize = GameMaster.Instance.SelectedIslandSize;

            CurrentIslandType = (IslandType) Random.Range(0, Enum.GetNames(typeof(IslandType)).Length);
            
            GameMaster.Instance.CurrentIslandType = CurrentIslandType;
            
            var sceneId = Mathf.Clamp(smallIslandIndex + (int) islandSize, smallIslandIndex, largeIslandIndex);
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(smallIslandIndex - 1));
            SceneManager.LoadSceneAsync(sceneId, LoadSceneMode.Additive);
            hasLoadedIslands = true;
        }

        /// <summary>
        /// Triggers an canvas with an animation to display the island name and type.
        /// </summary>
        public void DisplayIslandType() {
            var textKeys = new List<string>();

            switch(GameMaster.Instance.SelectedIslandSize) {
                case IslandSizes.Small:
                    textKeys.Add(smallIslandTextKey);
                    break;
                case IslandSizes.Medium:
                    textKeys.Add(mediumIslandTextKey);
                    break;
                case IslandSizes.Large:
                    textKeys.Add(largeIslandTextKey);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            switch(CurrentIslandType) {
                case IslandType.BrawlIsland:
                    textKeys.Add(brawlTypeTextKey);
                    break;
                case IslandType.TreasureIsland:
                    textKeys.Add(treasureTypeTextKey);
                    break;
                case IslandType.MerchantIsland:
                    textKeys.Add(merchantTypeTextKey);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            Instantiate(locationNameCanvasPrefab).GetComponent<CanvasPopupLocationName>().SetUpLocationCanvas(textKeys);
        }
        
        /// <summary>
        /// Loads an island scene based on GameMaster settings.
        /// </summary>
        public void UnloadIslands() {
            if(SceneManager.GetSceneByBuildIndex(smallIslandIndex).isLoaded) SceneManager.UnloadSceneAsync(smallIslandIndex);
            if(SceneManager.GetSceneByBuildIndex(mediumIslandIndex).isLoaded) SceneManager.UnloadSceneAsync(mediumIslandIndex);
            if(SceneManager.GetSceneByBuildIndex(largeIslandIndex).isLoaded) SceneManager.UnloadSceneAsync(largeIslandIndex);
        }
    }
}