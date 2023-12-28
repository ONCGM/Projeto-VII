using System;
using System.Collections;
using System.Collections.Generic;
using UI.Localization;
using UnityEngine;

namespace Localization {
    /// <summary>
    /// Loads and reads localization values based on a given language.
    /// </summary>
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
        private static Dictionary<string, string> localizedPtBr = new Dictionary<string, string>();
        
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
        /// CSV file loader.
        /// </summary>
        public static CSVLoader Loader { get; private set; }

        /// <summary>
        /// Initializes the system and loads language values.
        /// </summary>
        public static void Initialize() {
            Loader = new CSVLoader();
            Loader.LoadCSVFile();

            UpdateLanguageDictionaries();

            InitializationComplete = true;
        }
        
        /// <summary>
        /// Reloads the language dictionaries values.
        /// </summary>
        public static void UpdateLanguageDictionaries() {
            localizedEn = Loader.GetLanguageValues("en");
            
            localizedPtBr = Loader.GetLanguageValues("pt_br");
            
            localizedJp = Loader.GetLanguageValues("jp");
        }

        /// <summary>
        /// Returns a text string based on the given key.
        /// </summary>
        public static string GetLocalizedValue(string key) {
            if(!InitializationComplete) Initialize();

            var value = key;

            switch(CurrentLanguage) {
                case Language.Portuguese_Brazil:
                    if(!localizedPtBr.TryGetValue(key, out value)) { value = "☹"; }
                    break;
                case Language.English:
                    if(!localizedEn.TryGetValue(key, out value)) { value = "☹"; }
                    break;
                case Language.Japanese:
                    if(!localizedJp.TryGetValue(key, out value)) { value = "☹"; }
                    break;
            }

            return value;
        }
        
    #if UNITY_EDITOR
        /// <summary>
        /// Adds an entry to the localization file in the Portuguese language.
        /// </summary>
        /// <param name="key"> Key of the entry. </param>
        /// <param name="value"> Portuguese text of the entry. </param>
        public static void AddEntry(string key, string value) {
            var replace = value;
            
            if(value.Contains("\"")) {
                 replace = value.Replace('"', '\"');
            }
            
            if(Loader is null) Loader = new CSVLoader();
            
            Loader.LoadCSVFile();
            Loader.AddEntry(key, value);
            Loader.LoadCSVFile();
            
            UpdateLanguageDictionaries();
        }
        
        /// <summary>
        /// Edits an entry to the localization file in the Portuguese language.
        /// </summary>
        /// <param name="key"> Key of the entry. </param>
        /// <param name="value"> Portuguese text of the entry. </param>
        public static void EditEntry(string key, string value) {
            var replace = value;
            
            if(value.Contains("\"")) {
                replace = value.Replace('"', '\"');
            }
            
            if(Loader is null) Loader = new CSVLoader();
            
            Loader.LoadCSVFile();
            Loader.EditEntry(key, value);
            Loader.LoadCSVFile();
            
            UpdateLanguageDictionaries();
        }
        
        /// <summary>
        /// Removes an entry to the localization file.
        /// </summary>
        /// <param name="key"> Key of the entry. </param>
        public static void RemoveEntry(string key) {
            if(Loader is null) Loader = new CSVLoader();
            
            Loader.LoadCSVFile();
            Loader.RemoveEntry(key);
            Loader.LoadCSVFile();
            
            UpdateLanguageDictionaries();
        }

        /// <summary>
        /// Returns the complete Portuguese language dictionary for editor usage.
        /// </summary>
        public static Dictionary<string, string> GetLanguageDictionary() {
            if(!InitializationComplete) { Initialize(); }

            return localizedPtBr;
        }
        
    #endif
    }
}