using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Other {
    /// <summary>
    /// Holds an event for helping with triggering actions within other scripts.
    /// </summary>
    public class CustomEventTrigger : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Event(s) to Trigger")]
        [SerializeField] private UnityEvent customEvent;

        [Header("Trigger Options")] 
        [SerializeField] private bool triggerOnAwake;
        [SerializeField] private bool triggerOnStart;
        [SerializeField] private bool triggerOnCall = true;
        [SerializeField, Range(1, 25)] private int repeatCount = 1;
        
        #pragma warning restore 0649

        // Unity Events.
        private void Awake() { if(triggerOnAwake) CallEvent(); }
        private void Start() { if(triggerOnStart) CallEvent(); }

        /// <summary>
        /// Invokes the event based on settings.
        /// </summary>
        public void InvokeEvent() { if(triggerOnCall) CallEvent(); }

        /// <summary>
        /// Triggers the event for however many times specified.
        /// </summary>
        private void CallEvent() { for(var i = 0; i < repeatCount; i++) { customEvent?.Invoke(); } }
    }
}