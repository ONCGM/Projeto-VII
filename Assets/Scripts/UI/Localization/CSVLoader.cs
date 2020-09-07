using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UI.Localization {
    /// <summary>
    /// Loads a csv file and parses language from it.
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
    }
}