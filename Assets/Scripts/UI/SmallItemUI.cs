using System.Collections;
using DG.Tweening;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    /// <summary>
    /// Populates a small format item ui.
    /// </summary>
    public class SmallItemUI : MonoBehaviour {
        #pragma warning disable 0649
        // Fields and Properties.
        public InventoryItemEntry itemInfo { get; private set; }
        
        // Components.
        [Header("UI Components")]
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amount;

        #pragma warning restore 0649
        // Gets references.
        private void Awake() {
            if(icon is null) icon = GetComponentInChildren<Image>();
            if(amount is null) amount = transform.GetComponentInChildren<TMP_Text>();
        }

        /// <summary>
        /// Sets up the item ui with the item information.
        /// </summary>
        public void SetUpItemUi(InventoryItemEntry item, float animationTime) {
            itemInfo = item;
            
            icon.sprite = itemInfo.ItemSettings.itemImage;
            
            // ReSharper disable once SpecifyACultureInStringConversionExplicitly
            amount.text = itemInfo.Stack.ToString();

            if(animationTime > 0f) {
                transform.localScale = Vector3.zero;

                transform.DOScale(Vector3.one, animationTime);
            }
        }
    }
}