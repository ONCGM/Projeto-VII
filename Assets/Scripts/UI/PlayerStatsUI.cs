using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Player;
using TMPro;
using UnityEngine;

namespace UI {
    public class PlayerStatsUI : MonoBehaviour {
        #pragma warning disable 0649
        [SerializeField] private TMP_Text health, stamina;
        private PlayerController player;
        
        #pragma warning restore 0649
        
        private void Awake() {
            player = FindObjectOfType<PlayerController>();
        }

        private void Update() {
            health.text = $"Health: {player.Health}";
            stamina.text = $"Stamina: {player.Stamina}";
        }
    }
}