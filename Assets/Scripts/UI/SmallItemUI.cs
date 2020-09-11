using System.Collections;
using System.Collections.Generic;
using Items;
using Localization;
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

        private void Awake() {
            if(icon is null) icon = GetComponentInChildren<Image>();
            if(amount is null) amount = transform.GetComponentInChildren<TMP_Text>();
        }

        public void SetUpItemUi(InventoryItemEntry item) {
            itemInfo = item;
            
            icon.sprite = itemInfo.ItemSettings.itemImage;
            amount.text = itemInfo.Stack.ToString();
        }
    }
}