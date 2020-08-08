using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
    public class GameMaster : MonoBehaviour {

        private object gameState;

        public object GameState {
            get => gameState;
            set => gameState = value;
        }

        private object playerStats;

        public object PlayerStats {
            get => playerStats;
            set => playerStats = value;
        }

        private void Awake() {
            if(GameObject.FindObjectsOfType<GameMaster>().Length > 1) {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }
    }
}