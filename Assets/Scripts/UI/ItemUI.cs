using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    /// <summary>
    /// Populates a full size item ui.
    /// </summary>
    public class ItemUI : MonoBehaviour {
        #pragma warning disable 0649
        // Fields and Properties.
        public InventoryItemEntry itemInfo { get; private set; }
        
        // Components.
        [Header("UI Components")]
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text title, description;
        [SerializeField] private Button consume, discard; 

        #pragma warning restore 0649

        private void Awake() {
            if(icon is null) icon = GetComponentInChildren<Image>();
            if(title is null) title = transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>();
            if(description is null) description = transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>();
            if(consume is null) consume = transform.GetChild(2).GetChild(0).GetComponent<Button>();
            if(discard is null) discard = transform.GetChild(2).GetChild(1).GetComponent<Button>();
        }

        /// <summary>
        /// Sets up the item UI.
        /// </summary>
        public void SetUpItemUi(InventoryItemEntry item) {
            itemInfo = item;
            
            icon.sprite = itemInfo.ItemSettings.itemImage;
            title.text = $"{LocalizationSystem.GetLocalizedValue(itemInfo.ItemSettings.itemNameKey)} x {itemInfo.Stack}";
            description.text = item.ItemSettings.hasEffect
                                   ? $"{LocalizationSystem.GetLocalizedValue(itemInfo.ItemSettings.itemDescriptionKey)} " +
                                     $"{Environment.NewLine} {LocalizationSystem.GetLocalizedValue(itemInfo.ItemSettings.itemEffects)}"
                                   : LocalizationSystem.GetLocalizedValue(itemInfo.ItemSettings.itemDescriptionKey);

            if(item.ItemSettings.hasEffect) {
                consume.onClick.AddListener(item.ItemSettings.ApplyEffect);
            } else {
                Destroy(consume.gameObject);
            }
            
            discard.onClick.AddListener(item.ItemSettings.DropItem);
        }

        /// <summary>
        /// Locks the buttons on the ui.
        /// </summary>
        public void LockButtons(bool state) {
            if(consume != null) consume.interactable = !state;
            if(discard != null) discard.interactable = !state;
        }
    }
}