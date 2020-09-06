using System;
using System.Collections;
using System.Collections.Generic;
using UI.Localization;
using UnityEngine;

namespace Localization {
    public static class LocalizationSystem {
        /// <summary>
        /// Enum defining the languages used in this project.
        /// </summary>
        public enum Language {
            Portuguese_Brazil,
            English,
            Japanese
        }

        /// <summary>
        /// Current language set by the user, defaults as portuguese.
        /// </summary>
        public static Language CurrentLanguage {
            get => currentLanguage;
            set {
                currentLanguage = value; 
                OnLanguageUpdate?.Invoke();
            }
        }

        // Language dictionaries.
        private static Dictionary<string, string> localizedPt_Br = new Dictionary<string, string>();
        
        private static Dictionary<string, string> localizedEn = new Dictionary<string, string>();
        
        private static Dictionary<string, string> localizedJp = new Dictionary<string, string>();

        // Events.
        public static Action OnLanguageUpdate;
        private static Language currentLanguage = Language.Portuguese_Brazil;

        /// <summary>
        /// Has the system been initialized.
        /// </summary>
        public static bool InitializationComplete { get; private set; }

        /// <summary>
        /// Initializes the system and loads language values.
        /// </summary>
        public static void Initialize() {
            var loader = new CSVLoader();
            loader.LoadCSVFile();

            localizedPt_Br = loader.GetLanguageValues("pt_br");
            
            localizedPt_Br = loader.GetLanguageValues("en");
            
            localizedPt_Br = loader.GetLanguageValues("jp");

            InitializationComplete = true;
        }

        /// <summary>
        /// Returns a localized string based on the given key.
        /// </summary>
        public static string GetLocalizedValue(string key) {
            if(!InitializationComplete) Initialize();

            var value = key;

            switch(CurrentLanguage) {
                case Language.Portuguese_Brazil:
                    localizedPt_Br.TryGetValue(key, out value);
                    break;
                case Language.English:
                    localizedEn.TryGetValue(key, out value);
                    break;
                case Language.Japanese:
                    localizedJp.TryGetValue(key, out value);
                    break;
            }

            return value;
        }
    }
}