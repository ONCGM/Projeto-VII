using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Islands {
    /// <summary>
    /// Places a random piece of scenery in the determined spots.
    /// </summary>
    public class IslandSceneryObjectRandomizer : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")]
        [SerializeField] private List<GameObject> sceneryObjects = new List<GameObject>();

        [SerializeField] private bool useRandomRotation = true;
        [SerializeField] private Vector2 scaleRandomization = Vector2.one;
        [SerializeField, Range(0f, 1f)] private float chanceOfNotSpawningObject = 0.1f;
        #pragma warning restore 0649

        /// <summary>
        /// Spawns a randomly selected piece of scenery.
        /// </summary>
        private void Start() {
            if(sceneryObjects.Count < 1) { return; }

            if(Random.value < chanceOfNotSpawningObject) { return; }

            var scenery = Instantiate(sceneryObjects[Random.Range(0, sceneryObjects.Count)], transform.position, Quaternion.Euler(0f, useRandomRotation ? Random.Range(0f, 361f) : 0f,0f));
            scenery.transform.localScale = Vector3.one * Random.Range(scaleRandomization.x, scaleRandomization.y);
        }
    }
}