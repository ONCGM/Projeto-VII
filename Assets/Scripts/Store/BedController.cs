using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Entity.Player;
using Game;
using UI.Localization;
using UI.Popups;
using UnityEngine;
using UnityEngine.AI;
using static UI.Popups.CanvasPopupDialog;

namespace Store {
    /// <summary>
    /// Allows the player to sleep.
    /// </summary>
    public class BedController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private float fadeAnimationTime = 5f;
        [SerializeField] private float sleepTriggerCooldown = 1f;
        
        [Header("References")]
        [SerializeField] private Transform wakeUpPosition;
        
        [Header("Localization Keys")] 
        [SerializeField] private LocalizedString titleKey;
        [SerializeField] private LocalizedString messageKey, 
                                                 confirmKey,
                                                 cancelKey;
        
        [Header("Prefabs")] 
        [SerializeField] private GameObject popupPrefab;
        
        // Variables.
        private CanvasGroup canvasGroup;
        private PlayerController player;
        public bool canTriggerSleep = true;
        
        private CanvasPopupDialog popupDialog;
        private List<ButtonSettings> sleepButtons = new List<ButtonSettings> ();
        #pragma warning restore 0649
        
        // Class Setup.
        private void Awake() {
            canvasGroup = transform.parent.GetComponentInChildren<CanvasGroup>();
            canvasGroup.interactable = false;
            player = FindObjectOfType<PlayerController>();
            sleepButtons.Add(new ButtonSettings(confirmKey.key, PopupButtonHighlight.Highlight, 0));
            sleepButtons.Add(new ButtonSettings(cancelKey.key, PopupButtonHighlight.Normal, 1));
        }

        // Collision Detection.
        private void OnTriggerEnter(Collider other) {
            if(other.CompareTag(playerTag) && canTriggerSleep) StartSleep();
        }

        /// <summary>
        /// Triggers the sleep popup.
        /// </summary>
        public void StartSleep() {
            if(popupDialog != null) return; 
                
            popupDialog = Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();

            canTriggerSleep = false;
            
            canvasGroup.interactable = true;
            popupDialog.SetUpPopup(titleKey.key, messageKey.key, sleepButtons, ExecutionState.PopupPause, i => {
                if(i == 0) Sleep();
                else Invoke(nameof(SleepCooldown), sleepTriggerCooldown);
            });
        }

        /// <summary>
        /// Waits a bit until to allow the player
        /// to try to sleep again.
        /// </summary>
        private void SleepCooldown() {
            canTriggerSleep = true;
            canvasGroup.interactable = false;
        }

        /// <summary>
        /// Makes the day pass.
        /// </summary>
        private void Sleep() {
            canvasGroup.interactable = false;

            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, fadeAnimationTime * 0.5f).onComplete +=
                PassDay;
        }

        /// <summary>
        /// Passing the day logic.
        /// </summary>
        private void PassDay() {
            if(GameMaster.Instance.CurrentTimeOfDay == TimeOfDay.Night) GameMaster.Instance.AdvanceOneTimePeriod();
            else {
                GameMaster.Instance.CurrentGameDay++;
                GameMaster.Instance.CurrentTimeOfDay = TimeOfDay.Morning;
            }

            var agent = player.GetComponent<NavMeshAgent>();
            agent.enabled = false;
            player.transform.position = wakeUpPosition.position;
            player.transform.rotation = wakeUpPosition.rotation;
            player.Health = player.MaxHealth;
            GameMaster.Instance.SaveGame();

            DOTween.To(() => canvasGroup.alpha,
                       x => canvasGroup.alpha = x,
                       0f, fadeAnimationTime * 0.5f).onComplete = () => {
                canTriggerSleep = true;
                agent.enabled = true;
            };
        }

        /// <summary>
        /// Animates the sleep sequence.
        /// </summary>
        private IEnumerator SleepRoutine() {
            while(canvasGroup.alpha > 0f) {
                canvasGroup.alpha -= fadeAnimationTime * Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            
            if(GameMaster.Instance.CurrentTimeOfDay == TimeOfDay.Night) GameMaster.Instance.AdvanceOneTimePeriod();
            else {
                GameMaster.Instance.CurrentGameDay++;
                GameMaster.Instance.CurrentTimeOfDay = TimeOfDay.Morning;
            }
            
            var agent = player.GetComponent<NavMeshAgent>();
            agent.enabled = false;
            player.transform.position = wakeUpPosition.position;
            player.transform.rotation = wakeUpPosition.rotation;
            player.Health = player.MaxHealth;
            GameMaster.Instance.SaveGame();
            
            while(canvasGroup.alpha < 1f) {
                canvasGroup.alpha += fadeAnimationTime * Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            
            canTriggerSleep = true;
            agent.enabled = true;
        }
    }
}