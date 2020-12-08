using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Islands {
    /// <summary>
    /// Randomly hides a path or island.
    /// </summary>
    public class IslandRandomizer : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")]
        [SerializeField, Range(0f, 1f)] private float chanceToSpawn = 0.5f;
        [Header("Spawn Depends On:")]
        [SerializeField] private List<IslandRandomizer> spawnDependencies = new List<IslandRandomizer>();

        // Properties.
        private List<NavMeshObstacle> navObstacles = new List<NavMeshObstacle>();
        private List<IslandPopulator> populators = new List<IslandPopulator>();
        private List<IslandSceneryObjectRandomizer> sceneryRandomizers = new List<IslandSceneryObjectRandomizer>();
        private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
        
        // Fields.
        /// <summary>
        /// Has this randomizer spawned its island or path.
        /// </summary>
        public bool Spawned { get; private set; }
        public bool HasDependencies { get; private set; }
        #pragma warning restore 0649

        /// <summary>
        /// Decides whether or not to hide this island or path.
        /// </summary>
        private void Awake() {
            Spawned = Random.value < chanceToSpawn;
            HasDependencies = spawnDependencies.Count > 0;
            
            navObstacles = GetComponentsInChildren<NavMeshObstacle>().ToList();

            foreach(var obstacle in navObstacles) {
                obstacle.enabled = !Spawned || HasDependencies;
            }

            if(Spawned && !HasDependencies) return; 

            populators = GetComponentsInChildren<IslandPopulator>().ToList();
            foreach(var populator in populators) {
                populator.enabled = false;
            }
            
            sceneryRandomizers = GetComponentsInChildren<IslandSceneryObjectRandomizer>().ToList();
            foreach(var sceneryRandomizer in sceneryRandomizers) {
                sceneryRandomizer.enabled = false;
            }
            
            meshRenderers = GetComponentsInChildren<MeshRenderer>().ToList();
            foreach(var meshRenderer in meshRenderers) {
                meshRenderer.enabled = false;
            }
        }

        /// <summary>
        /// Spawns or Hides the island or path.
        /// Also enables other scripts to populate the islands.
        /// </summary>
        private void Start() {
            if(!Spawned) return;
            
            if(!spawnDependencies.TrueForAll(x => x.Spawned)) return;
            
            foreach(var obstacle in navObstacles) {
                obstacle.enabled = false;
            }

            foreach(var populator in populators) {
                populator.enabled = true;
            }
            
            foreach(var sceneryRandomizer in sceneryRandomizers) {
                sceneryRandomizer.enabled = true;
            }
            
            foreach(var meshRenderer in meshRenderers) {
                meshRenderer.enabled = true;
            }
        }
    }
}