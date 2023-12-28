using System.Collections;
using System.Collections.Generic;
using Localization;
using UnityEngine;

namespace UI.Localization {
    /// <summary>
    /// A modified string with extra features for localization.
    /// </summary>
    [System.Serializable]
    public struct LocalizedString {
        /// <summary>
        /// Key of the string in the localization sheet.
        /// </summary>
        public string key;

        public LocalizedString(string key) {
            this.key = key;
        }
        
        /// <summary>
        /// Value of the localized string.
        /// </summary>
        public string value => LocalizationSystem.GetLocalizedValue(key);

        public static implicit operator LocalizedString(string key) {
            return new LocalizedString(key);
        }
    }
}