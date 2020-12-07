using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using Store;
using UI.Localization;
using UI.Popups;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI {
    /// <summary>
    /// Adds the option to close the store.
    /// </summary>
    public class CloseStoreUI : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")]
        [SerializeField, Range(0.1f, 5f)] private float fadeAnimationTime = 1f;
        [SerializeField] private GameObject firstSelected;
        
        [Header("Localization")] 
        [SerializeField] private LocalizedString closeStoreTitleKey;

        [SerializeField] private LocalizedString closeStoreMessageKey,
                                                 confirmButtonKey,
                                                 cancelButtonKey;
        
        [Header("Prefabs")] 
        [SerializeField] private GameObject popupPrefab;
        
        private List<CanvasPopupDialog.ButtonSettings> confirmCancelButtons =
            new List<CanvasPopupDialog.ButtonSettings>();

        private CanvasPopupDialog popupDialog;
        private CanvasGroup mainCanvasGroup;
        private Button closeButton;
        public bool IsOpen { get; private set; }
        #pragma warning restore 0649
        
        // Gets references and shit.
        private void Awake() {
            mainCanvasGroup = GetComponent<CanvasGroup>();
            closeButton = GetComponentInChildren<Button>();
            mainCanvasGroup.alpha = 0f;
            confirmCancelButtons = new List<CanvasPopupDialog.ButtonSettings>() {
                new CanvasPopupDialog.ButtonSettings(confirmButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Normal, 0),
                new CanvasPopupDialog.ButtonSettings(cancelButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Highlight, 1),
            };
        }

        /// <summary>
        /// Displays the button to close the store.
        /// </summary>
        public void DisplayUi() {
            DOTween.To(() => mainCanvasGroup.alpha, x => mainCanvasGroup.alpha = x, 1f, fadeAnimationTime);
            IsOpen = true;
            closeButton.interactable = true;
            
            foreach(var eventSystem in FindObjectsOfType<EventSystem>()) {
                eventSystem.SetSelectedGameObject(firstSelected);
            }
        }


        /// <summary>
        /// Hides the button to close the store.
        /// </summary>
        public void HideUi() {
            DOTween.To(() => mainCanvasGroup.alpha, x => mainCanvasGroup.alpha = x, 0f, fadeAnimationTime);
            IsOpen = false;
            closeButton.interactable = false;
        }

        /// <summary>
        /// Starts the store closing sequence.
        /// </summary>
        public void TriggerStoreClose() {
            if(popupDialog != null) return;
            closeButton.interactable = false;
            
            popupDialog = Instantiate(popupPrefab).GetComponent<CanvasPopupDialog>();
            popupDialog.SetUpPopup(closeStoreTitleKey.key,
                                   closeStoreMessageKey.key,
                                   confirmCancelButtons, ExecutionState.PopupPause, i => {
                                       if(i == 0) {
                                           CloseStore();
                                       } 
                                       
                                       closeButton.interactable = true;
                                   });
        }

        /// <summary>
        /// Closes the store.
        /// </summary>
        private void CloseStore() {
            HideUi();
            FindObjectOfType<StoreController>().CloseEarly();
        }
    }
}