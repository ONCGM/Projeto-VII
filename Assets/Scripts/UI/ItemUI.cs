using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
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

        public void SetUpItemUi(InventoryItemEntry item) {
            itemInfo = item;
            
            icon.sprite = itemInfo.ItemSettings.itemImage;
            title.text = $"{itemInfo.ItemSettings.itemName} x {itemInfo.Stack}";
            description.text = itemInfo.ItemSettings.itemDescription;
            consume.enabled = false;
            discard.enabled = false;
        }
    }
}