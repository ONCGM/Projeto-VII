using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI.Localization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UI {
    /// <summary>
    /// Displays some loading hints to help the players.
    /// </summary>
    public class LoadingHints : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Localization")] 
        [SerializeField] private LocalizedString objectiveKey;
        [SerializeField] private List<LocalizedString> loadingHintKeys = new List<LocalizedString>();

        [Header("UI Components")] 
        [SerializeField] private TMP_Text hintsText;
        [SerializeField] private TMP_Text objectiveText;

        [Header("Settings")] 
        [SerializeField, Range(0.01f, 2f)] private float delayBetweenEachCharacter = 0.2f;
        [SerializeField, Range(1f, 15f)] private float delayBetweenEachHint = 4f;


        private WaitForSeconds waitFrame;
        private WaitForSeconds waitForHint;
        #pragma warning restore 0649
        
        // Sets up the class.
        private void Awake() {
            if(hintsText == null) transform.GetChild(0).GetComponent<TMP_Text>();
            if(objectiveText == null) transform.GetChild(1).GetComponent<TMP_Text>();
            waitFrame = new WaitForSeconds(delayBetweenEachCharacter);
            waitForHint = new WaitForSeconds(delayBetweenEachHint);
        }

        // Sets up the objective text.
        private IEnumerator Start() {
            objectiveText.text = string.Empty;
            
            foreach(var character in objectiveKey.value.ToCharArray()) {
                objectiveText.text += character;
                yield return waitFrame;
            }

            StartCoroutine(nameof(GenerateHints));
        }

        /// <summary>
        /// Generates the hints and changes between them.
        /// </summary>
        private IEnumerator GenerateHints() {
            hintsText.text = string.Empty;
            
            foreach(var character in loadingHintKeys[Random.Range(0, loadingHintKeys.Count)].value.ToCharArray()) {
                hintsText.text += character;
                yield return waitFrame;
            }

            yield return waitForHint;

            StartCoroutine(nameof(GenerateHints));
        }
    }
}