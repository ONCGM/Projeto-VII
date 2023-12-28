using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UI {
    /// <summary>
    /// Opens and closes the in game menu.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class OpenCloseGameMenu : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")] 
        [SerializeField, Range(0.01f, 3f)] private float fadeAnimationSpeed = 0.7f;
        [SerializeField] private GameObject firstSelected;
        
        // Components
        private ActionInputs inputs;
        private bool isOpen { get; set; }
        private Canvas gameMenuCanvas;
        private CanvasGroup canvasGroup;
        
        #pragma warning restore 0649

        private void Awake() {
            inputs = new ActionInputs();
            inputs.Player.Menu.performed += OpenCloseMenu;
            gameMenuCanvas = GetComponentInParent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        // Opens and closes the menu.
        private void OpenCloseMenu(InputAction.CallbackContext callbackContext) {
            isOpen = !isOpen;
            canvasGroup.blocksRaycasts = isOpen;
            GameMaster.Instance.GameMenuIsOpen = isOpen;
            if(isOpen) {
                foreach(var eventSystem in FindObjectsOfType<EventSystem>()) {
                    eventSystem.SetSelectedGameObject(firstSelected);
                }
            }
            DOTween.To(()=> canvasGroup.alpha, x=> canvasGroup.alpha = x, (GameMaster.Instance.GameMenuIsOpen ? 1f : 0f), fadeAnimationSpeed);
        }
        
        // OnEnable Unity Event, enables input.
        private void OnEnable() {
            inputs.Enable();
        }

        // OnDisable Unity Event, disables input.
        private void OnDisable() {
            inputs.Disable();
        }

        // Disposes and disables the input.
        private void OnDestroy() {
            inputs.Player.Menu.performed -= OpenCloseMenu;
            inputs.Dispose();
        }
    }
}