using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using Localization;
using UI.Localization;
using UI.Menu;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace UI.Popups {
    public class CanvasPopupDialog : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")]
        [SerializeField, Range(0.1f, 3f)] private float popinAnimationDuration = 0.75f;
        
        [Header("Prefabs")]
        [SerializeField] private List<GameObject> popupButtonPrefabs = new List<GameObject>();

        [Header("Components")]
        [SerializeField] private TextMeshLocalizer titleText;
        [SerializeField] private TextMeshLocalizer messageText;
        [SerializeField] private Transform buttonParent;
        private List<Button> buttons = new List<Button>();
        
        // Fields and Properties.
        private bool popupOpen;
        private WaitForEndOfFrame waitFrame;
            
        /// <summary>
        /// Defines what type of highlight to use in a popup button.
        /// </summary>
        public enum PopupButtonHighlight {
            Normal,
            Highlight,
            StrongHighlight
        }
        
        /// <summary>
        /// Helper for instantiating popup buttons.
        /// </summary>
        public struct ButtonSettings {
            public string key;
            public PopupButtonHighlight highlightLevel;
            public int id;
            
            public ButtonSettings(string key, PopupButtonHighlight highlightLevel, int id) {
                this.key = key;
                this.highlightLevel = highlightLevel;
                this.id = id;
            }
        }
        
        #pragma warning restore 0649

        /// <summary>
        /// Animates the popup.
        /// </summary>
        private void AnimateIn() {
            transform.GetChild(0).localScale = Vector3.zero;
            transform.GetChild(0).DOScale(Vector3.one, popinAnimationDuration).onComplete += () => {
                foreach(var eventSystem in FindObjectsOfType<EventSystem>()) {
                    eventSystem.UpdateModules();
                    eventSystem.SetSelectedGameObject(buttons[0].gameObject);
                    eventSystem.UpdateModules();
                }
                
                foreach(var inputModule in FindObjectsOfType<InputSystemUIInputModule>()) {
                    inputModule.enabled = false;
                    inputModule.UpdateModule();
                    inputModule.enabled = true;
                    inputModule.DeactivateModule();
                    inputModule.ActivateModule();
                    inputModule.UpdateModule();
                }
            };
        }

        /// <summary>
        /// Sets the popup to have the desired values. Use localization keys to get the text.
        /// </summary>
        /// <param name="titleKey"> Localization key of the title. </param>
        /// <param name="messageKey"> Localization key of the message. </param>
        /// <param name="buttonSettings"> Buttons of the popup. Max of 5 buttons.
        /// Should follow order: Confirm, extras and then cancel.</param>
        /// <param name="popupExecutionState"> Does the game need to be paused? </param>
        /// <param name="callback"> Function to call when a button is pressed. </param>
        public void SetUpPopup(string titleKey, string messageKey, List<ButtonSettings> buttonSettings,
                               ExecutionState popupExecutionState, Action<int> callback) {
            popupOpen = true;
            titleText.UpdateKey(titleKey);
            messageText.UpdateKey(messageKey);
            
            buttons.Clear();

            foreach(var settings in buttonSettings) {
                var button = Instantiate(popupButtonPrefabs[(int) settings.highlightLevel], buttonParent).GetComponent<Button>();
                
                button.onClick.AddListener(() => callback(settings.id));
                button.onClick.AddListener(() => popupOpen = false);
                button.GetComponentInChildren<TextMeshLocalizer>().UpdateKey(settings.key);
                
                buttons.Add(button);
            }

            GameMaster.Instance.GameState = popupExecutionState;

            AnimateIn();
            
            StartCoroutine(nameof(PopupPauseRoutine));
        }

        /// <summary>
        /// Sets the popup to have the desired values. Use localization keys to get the text.
        /// </summary>
        /// <param name="titleKey"> Localization key of the title. </param>
        /// <param name="messageKey"> Localization key of the message. </param>
        /// <param name="additionalMessageKey"> Extra localization key for a second line message.</param>
        /// <param name="buttonSettings"> Buttons of the popup. Max of 5 buttons.
        /// Should follow order: Confirm, extras and then cancel.</param>
        /// <param name="popupExecutionState"> Does the game need to be paused? </param>
        /// <param name="callback"> Function to call when a button is pressed. </param>
        public void SetUpPopup(string titleKey, string messageKey, string additionalMessageKey, List<ButtonSettings> buttonSettings,
                               ExecutionState popupExecutionState, Action<int> callback) {
            popupOpen = true;
            titleText.UpdateKey(titleKey);
            messageText.TextMeshComponent.text = 
                $"{LocalizationSystem.GetLocalizedValue(messageKey)} " +
                $"{Environment.NewLine} " +
                $"{(string.IsNullOrWhiteSpace(additionalMessageKey) ? string.Empty : LocalizationSystem.GetLocalizedValue(additionalMessageKey))}";

            buttons.Clear();

            foreach(var settings in buttonSettings) {
                var button = Instantiate(popupButtonPrefabs[(int) settings.highlightLevel], buttonParent).GetComponent<Button>();
                
                button.onClick.AddListener(() => callback(settings.id));
                button.onClick.AddListener(() => popupOpen = false);
                button.GetComponentInChildren<TextMeshLocalizer>().UpdateKey(settings.key);
                
                buttons.Add(button);
            }

            GameMaster.Instance.GameState = popupExecutionState;

            AnimateIn();
            
            StartCoroutine(nameof(PopupPauseRoutine));
        }

        /// <summary>
        /// Waits for a button to be pressed, disables all buttons and then resumes the game execution.
        /// </summary>
        private IEnumerator PopupPauseRoutine() {
            waitFrame = new WaitForEndOfFrame();
            
            while(popupOpen) {
                yield return waitFrame;
            }

            foreach(var button in buttons) {
                button.gameObject.SetActive(false);
            }

            ClosePopup();
        }

        /// <summary>
        /// Closes the popup with an animation and then tags it for destruction.
        /// </summary>
        private void ClosePopup() {
            if(FindObjectOfType<MainMenuController>()) FindObjectOfType<MainMenuController>().SetEventSystem();
            GameMaster.Instance.GameState = ExecutionState.Normal;
            transform.GetChild(0).DOScale(Vector3.zero, popinAnimationDuration * .7f).onComplete += () => { Destroy(gameObject);};
        }
    }
}