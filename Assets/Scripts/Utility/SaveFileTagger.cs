using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

namespace Utility {
    /// <summary>
    /// Tags a save file as not new.
    /// </summary>
    public class SaveFileTagger : MonoBehaviour {
        // Tags the file then self-destructs.
        private void Awake() {
            GameMaster.Instance.MasterSaveData.brandSpankingNewSave = false;
            GameMaster.Instance.SaveGame();
            Destroy(gameObject);
        }
    }
}