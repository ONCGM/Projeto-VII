using System;
using System.Collections.Generic;
using Localization;
using UnityEditor;
using UnityEngine;

namespace UI.Localization {
    /// <summary>
    /// Window editor for the text localizer.
    /// </summary>
    public class LocalizerEditorWindow : EditorWindow {
        public string key;
        public string value;
        
        public static void Open(string key) {
            var window = ScriptableObject.CreateInstance<LocalizerEditorWindow>();
            window.titleContent = new GUIContent("Localization Window");
            window.ShowUtility();
            window.key = key;
        }
        
        public void OnGUI() {
            key = EditorGUILayout.TextField("Key: ", key);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Value:", GUILayout.MaxWidth(50));

            EditorStyles.textArea.wordWrap = true;
            value = EditorGUILayout.TextArea(value, EditorStyles.textArea,
                                             GUILayout.Height(100), GUILayout.Width(400));
            EditorGUILayout.EndHorizontal();

            if(GUILayout.Button("Add")) {
                if(!LocalizationSystem.GetLocalizedValue(key).Equals(string.Empty)) {
                    LocalizationSystem.EditEntry(key, value);
                } else {
                    LocalizationSystem.AddEntry(key, value);
                }
            }
            
            minSize = new Vector2(460, 250);
            maxSize = minSize;
        }
    }

    /// <summary>
    /// A editor window to search for language entries.
    /// </summary>
    public class LocalizerSearchWindow : EditorWindow {
        public string value;
        public Vector2 scroll;
        public Dictionary<string, string> languageDictionary;
        public static LocalizedStringDrawer RequestingLocalizedStringDrawer;
        
        public static void Open(LocalizedStringDrawer localizedStringDrawer) {
            var window = ScriptableObject.CreateInstance<LocalizerSearchWindow>();
            window.titleContent = new GUIContent("Localization Search");

            var mousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            var rect = new Rect(mousePosition.x - 450,  mousePosition.y + 10, 10, 10);
            window.ShowAsDropDown(rect, new Vector2(500, 300));

            RequestingLocalizedStringDrawer = localizedStringDrawer;
        }

        private void OnEnable() {
            languageDictionary = LocalizationSystem.GetLanguageDictionary();
        }

        public void OnGUI() {
            EditorGUILayout.BeginHorizontal("Box");
            EditorGUILayout.LabelField("Search: ", EditorStyles.boldLabel);
            value = EditorGUILayout.TextField(value);
            EditorGUILayout.EndHorizontal();
            
            SearchForEntries();
        }

        private void SearchForEntries() {
            if(value is null) { return; }

            EditorGUILayout.BeginVertical();
            scroll = EditorGUILayout.BeginScrollView(scroll);

            foreach(var entry in languageDictionary) {
                if(entry.Key.ToLower().Contains(value.ToLower()) || entry.Value.ToLower().Contains(value.ToLower())) {
                    EditorGUILayout.BeginHorizontal("Box");

                    if(GUILayout.Button("R", GUILayout.MaxWidth(20), GUILayout.MaxHeight(20))) {
                        if(EditorUtility.DisplayDialog($"Remove key {entry.Key} ?", 
                                                       "This will delete the entry from the localization sheet permanently.",
                                                       "Delete it.", "Don't do it.")) {
                            LocalizationSystem.RemoveEntry(entry.Key);
                        }
                    }
                    
                    EditorGUILayout.TextField(entry.Key);
                    EditorGUILayout.LabelField(entry.Value);

                    if(GUILayout.Button("Select", GUILayout.MaxWidth(100), GUILayout.MaxHeight(20))) {
                        RequestingLocalizedStringDrawer.keyValue = entry.Key;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }
}