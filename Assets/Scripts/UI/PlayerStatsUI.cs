using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Player;
using TMPro;
using UnityEngine;

namespace UI {
    public class PlayerStatsUI : MonoBehaviour {
        [SerializeField] private TMP_Text health, stamina;
        private PlayerController player;
        
        private void Awake() {
            player = FindObjectOfType<PlayerController>();
        }

        private void Update() {
            health.text = $"Health: {player.Health}";
            stamina.text = $"Stamina: {player.Stamina}";
        }
    }
}