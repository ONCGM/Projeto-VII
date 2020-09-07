namespace Game {
    /// <summary>
    /// Holds info on the current game state and loaded save data.
    /// </summary>
    public struct GameState {
        /// <summary>
        /// Current execution state of the game.
        /// <para> Used to stop enemies if player gets a popup and
        /// similar scenarios.</para>
        /// </summary>
        public enum ExecutionState {
            Normal,
            PopUpPause,
            FullPause
        }

        //TODO
        public object SaveData { get; private set; }

        public void SetSaveData(object saveData) {
            SaveData = saveData;
        }
    }
}