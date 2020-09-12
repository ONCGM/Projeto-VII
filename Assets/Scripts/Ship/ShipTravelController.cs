using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ship {
    /// <summary>
    /// Controls the ship and its travelling routine.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class ShipTravelController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Scene Index")] 
        [SerializeField] private int townSceneIndex = 3;
        [SerializeField] private int travelSceneIndex = 4;
        [SerializeField] private int islandsSceneIndex = 5;
        
        
        // Components.
        private Animator anim;
        private static readonly int DepartToIsland = Animator.StringToHash("DepartToIsland");
        private static readonly int DepartToTown = Animator.StringToHash("DepartToTown");
        private static readonly int ArriveAtIsland = Animator.StringToHash("ArriveAtIsland");
        private static readonly int ArriveAtTown = Animator.StringToHash("ArriveAtTown");

        #pragma warning restore 0649

        // Sets the ship up.
        private void Awake() {
            anim = GetComponent<Animator>();
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Start the travel routine to the islands.
        /// </summary>
        public void StartTravelToIsland() {
            anim.SetTrigger(DepartToIsland);
            SceneManager.LoadScene(travelSceneIndex, LoadSceneMode.Additive);
        }
        // TODO:
        
        /// <summary>
        /// Starts the travel routine to the town.
        /// </summary>
        public void StartTravelToTown() { }
    }
}