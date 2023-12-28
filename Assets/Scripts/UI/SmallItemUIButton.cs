using System.Collections;
using DG.Tweening;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI {
    /// <summary>
    /// Populates a small format item ui.
    /// </summary>
    public class SmallItemUIButton : MonoBehaviour {
        #pragma warning disable 0649
        // Fields and Properties.
        public InventoryItemEntry itemInfo { get; private set; }
        
        // Components.
        [Header("UI Components")]
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amount;
        [SerializeField] private Button button;

        #pragma warning restore 0649
        // Gets references.
        private void Awake() {
            if(icon == null) icon = GetComponentInChildren<Image>();
            if(amount == null) amount = transform.GetComponentInChildren<TMP_Text>();
            if(button == null) button = GetComponent<Button>();
        }

        /// <summary>
        /// Sets up the item ui with the item information.
        /// </summary>
        public void SetUpItemUi(InventoryItemEntry item, float animationTime, UnityAction callback) {
            itemInfo = item;
            
            icon.sprite = itemInfo.ItemSettings.itemImage;

            // ReSharper disable once SpecifyACultureInStringConversionExplicitly
            amount.text = itemInfo.Stack.ToString();

            button.onClick.AddListener(callback);
            
            if(animationTime > 0f) {
                transform.localScale = Vector3.zero;

                transform.DOScale(Vector3.one, animationTime);
            }
        }
    }
}