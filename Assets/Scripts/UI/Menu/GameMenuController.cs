using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using UI.Localization;
using UI.Popups;
using UnityEngine;

namespace UI.Menu {
    /// <summary>
    /// Controls the in game menu. Very basic for now.
    /// </summary>
    public class GameMenuController : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Prefabs")] 
        [SerializeField] private GameObject popupPrefab;
        
        [Header("Localization")]
        [SerializeField] private LocalizedString gameSavedTitleKey;

        [SerializeField] private LocalizedString gameSavedMessageKey,
                                                 backToMenuTitleKey,
                                                 backToMenuMessageKey,
                                                 quitGameTitleKey,
                                                 quitGameMessageKey,
                                                 quitGameSaveProgressLostKey,
                                                 confirmButtonKey,
                                                 cancelButtonKey;
        
        private List<CanvasPopupDialog.ButtonSettings> confirmCancelButtons = new List<CanvasPopupDialog.ButtonSettings>();
        #pragma warning restore 0649

        // Set up of this class.
        private void Awake() {
            confirmCancelButtons = new List<CanvasPopupDialog.ButtonSettings>() {
                new CanvasPopupDialog.ButtonSettings(confirmButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Normal, 0),
                new CanvasPopupDialog.ButtonSettings(cancelButtonKey.key, CanvasPopupDialog.PopupButtonHighlight.Highlight, 1),
            };
        }

        /// <summary>
        /// Saves the game.
        /// </summary>
        public void SaveGame() {
            GameMaster.Instance.SaveGame();
            // TODO POPUP
        }

        /// <summary>
        /// Return to the main menu.
        /// </summary>
        public void ReturnToMainMenu() {
            // TODO POPUP & BEHAVIOUR
        }
        
        /// <summary>
        /// Quits the game.
        /// </summary>
        public void QuitGame() {
            // TODO POPUP & BEHAVIOUR
        }
    }
}