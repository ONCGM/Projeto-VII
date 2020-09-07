using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UI.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups {
    public class CanvasPopupDialog : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Prefabs")]
        [SerializeField] private GameObject popupButtonPrefab;

        [Header("Components")]
        [SerializeField] private TextMeshLocalizer titleText;
        [SerializeField] private TextMeshLocalizer messageText;
        [SerializeField] private Button confirmButton, cancelButton;
        private List<Button> extraButtons = new List<Button>();
        
        #pragma warning restore 0649

        /// <summary>
        /// Sets the popup to have the desired values. Use localization keys to get the text.
        /// </summary>
        /// <param name="titleKey"> Localization key of the title. </param>
        /// <param name="messageKey"> Localization key of the message. </param>
        /// <param name="extraButtonKeys"> Will the popup need extra buttons.
        /// Localization keys of the buttons. Max of 3 buttons. </param>
        /// <param name="hasCancel"> Will the popup have a cancel button? </param>
        /// <param name="callbacks"></param>
        public void SetUpPopup(string titleKey, string messageKey, IEnumerable<string> extraButtonKeys, bool hasCancel,
                                     List<Action> callbacks) {
            if(callbacks == null) throw new ArgumentNullException(nameof(callbacks));
            titleText.UpdateText(titleKey);
            messageText.UpdateText(messageKey);
            
            var index = 0;

            foreach(var key in extraButtonKeys) {
                if(index > 2) break;
                
                var button = Instantiate(popupButtonPrefab, transform);
                button.GetComponentInChildren<TextMeshLocalizer>().UpdateText(key);
                extraButtons.Add(button.GetComponent<Button>());
                
                index++;
            }
            
            confirmButton.onClick.AddListener(() => callbacks[0]());
            
            if(hasCancel) {
                cancelButton.onClick.AddListener(() => callbacks[1]());
                cancelButton.gameObject.SetActive(true);
            }

            for(var i = 0; i < extraButtons.Count; i++) {
                extraButtons[i].onClick.AddListener(() => callbacks[i + (hasCancel ? 2 : 1)]());
            }
        }
    }
}