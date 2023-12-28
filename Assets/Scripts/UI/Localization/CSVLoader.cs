using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UI.Localization {
    /// <summary>
    /// Loads a csv file and parses strings from it.
    /// </summary>
    public class CSVLoader {
        // Text and file.
        private TextAsset languagesFile;
        private readonly char lineSeparator = '\n';
        private readonly char surroundingCharacters = '"';
        private readonly string[] fieldSeparator = {"\",\""};
        private readonly string fileName = "Languages";

        /// <summary>
        /// Tries to load the languages file.
        /// </summary>
        public void LoadCSVFile() {
            languagesFile = Resources.Load<TextAsset>(fileName);
        }

        /// <summary>
        /// Gets the values for a given language id.
        /// </summary>
        /// <param name="languageId"> Id to get values for.</param>
        public Dictionary<string, string> GetLanguageValues(string languageId) {
            var dictionary = new Dictionary<string, string>();

            var lines = languagesFile.text.Split(lineSeparator);

            var index = -1;

            var headers = lines[0].Split(fieldSeparator, StringSplitOptions.None);

            for(var i = 0; i < headers.Length; i++) {
                if(!headers[i].Contains(languageId)) continue;
                index = i;
            }
            
            var parser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            for(var i = 0; i < lines.Length; i++) {
                var line = lines[i];

                var fields = parser.Split(line);

                for(var j = 0; j < fields.Length; j++) {
                    fields[j] = fields[j].TrimStart(' ', surroundingCharacters);
                    fields[j] = fields[j].TrimEnd(surroundingCharacters);
                    if(languageId.Equals("jp")) fields[j] = fields[j].Replace("\"", string.Empty);
                }

                if(fields.Length > index) {
                    var key = fields[0];

                    if(dictionary.ContainsKey(key)) {
                        continue;
                    }

                    var value = fields[index];

                    dictionary.Add(key, value);
                }
            }

            return dictionary;
        }
        
    #if UNITY_EDITOR
        /// <summary>
        /// Adds a new entry to the localization csv in the Portuguese language.
        /// </summary>
        /// <param name="key"> Name of the entry. </param>
        /// <param name="value"> Portuguese text of the entry. </param>
        public void AddEntry(string key, string value) {
            var append = $"\n\"{key}\",\"English\",\"{value}\",\"日本語\"";
            File.AppendAllText($"Assets/Resources/{fileName}.csv", append);
            
            UnityEditor.AssetDatabase.Refresh();
        }

        /// <summary>
        /// Removes an entry from the localization file.
        /// </summary>
        /// <param name="key"> Key of the entry to remove. </param>
        public void RemoveEntry(string key) {
            var lines = languagesFile.text.Split(lineSeparator);
         
            var keys = new string[lines.Length];

            for(var i = 0; i < lines.Length; i++) {
                var line = lines[i];

                keys[i] = line.Split(fieldSeparator, StringSplitOptions.None)[0];
            }

            var index = -1;

            for(var i = 0; i < keys.Length; i++) {
                if(keys[i].Contains(key)) {
                    index = i;
                    break;
                }
            }

            if(index <= 0) {
                Debug.LogFormat("Could not find \"{0}\" key in the localization file.", key);
                return;
            }

            var replacedLines = lines.Where(x => x != lines[index]).ToArray();

            var replacedFormatted = string.Join(lineSeparator.ToString(), replacedLines);
            
            File.WriteAllText($"Assets/Resources/{fileName}.csv", replacedFormatted);
        }

        /// <summary>
        /// Edits a given entry in the localization file.
        /// </summary>
        /// <param name="key"> Key of the entry. </param>
        /// <param name="value"> Portuguese text of the entry. </param>
        public void EditEntry(string key, string value) {
            RemoveEntry(key);
            AddEntry(key, value);
        }
    #endif
    }
}