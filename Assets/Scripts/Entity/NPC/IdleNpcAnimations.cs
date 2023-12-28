using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Entity.NPC {
    /// <summary>
    /// Plays some idling animations to make the
    /// npcs feel a little more alive.
    /// </summary>
    public class IdleNpcAnimations : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")]
        [SerializeField] private bool playRandomAnimations = true;
        [SerializeField] private bool playImmediateThink = false;
        [SerializeField] private bool playImmediateGrab = false;
        [SerializeField, Range(1f, 100f)] private float howOfterToTriggerAnimations = 25f;
        [SerializeField, Range(1f, 10f)] private float timeVariation = 5f;
        
        // Components.
        private Animator anim;
        
        // Variables and such.
        private WaitForSeconds waitForTrigger;
        private static readonly int Think = Animator.StringToHash("Think");
        private static readonly int GrabItem = Animator.StringToHash("Grab_Item");
        private static readonly int DropItem = Animator.StringToHash("Drop_Item");
        #pragma warning restore 0649

        #region Unity Events
        // Sets the class up.            
        private void Awake() {
            anim = GetComponentInChildren<Animator>();
            
            if(playImmediateThink) anim.SetTrigger(Think);
            if(playImmediateGrab) anim.SetTrigger(GrabItem);
            
            if(!playRandomAnimations) return;
            waitForTrigger = new WaitForSeconds(Random.Range(howOfterToTriggerAnimations - timeVariation, howOfterToTriggerAnimations + timeVariation));
            StartCoroutine(nameof(TriggerAnimations));
        }

        // Stops the coroutines.
        private void OnDestroy() {
            StopAllCoroutines();
        }

        #endregion
    
        /// <summary>
        /// Triggers the animation every so often.
        /// </summary>
        private IEnumerator TriggerAnimations() {
            yield return waitForTrigger;
            
            anim.SetTrigger(Think);
            
            if(playRandomAnimations) StartCoroutine(nameof(TriggerAnimations));
        }
    }
}