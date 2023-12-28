using System;
using System.Collections;
using System.Collections.Generic;
using Localization;
using UnityEditor;
using UnityEngine;

namespace UI.Localization {
    /// <summary>
    /// A custom property drawer for the text strings.
    /// </summary>
    [CustomPropertyDrawer(typeof(LocalizedString))]
    public class LocalizedStringDrawer : PropertyDrawer {
        private bool dropdown;
        private float height;
        public string keyValue = string.Empty;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if(dropdown) {
                return height + 25;
            }

            return 20;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            position.width -= 34;
            position.height = 18;
            
            var valueRect = new Rect(position);
            valueRect.x += 15;
            valueRect.width -= 15;

            var foldButtonRect = new Rect(position) {width = 15};

            dropdown = EditorGUI.Foldout(foldButtonRect, dropdown, string.Empty);

            position.x += 15;
            position.width -= 15;

            var key = property.FindPropertyRelative("key");
            key.stringValue = EditorGUI.TextField(position, key.stringValue);

            position.x += position.width + 2;
            position.width = 17;
            position.height = 17;

            if(GUI.Button(position, "F")) {
                LocalizerSearchWindow.Open(this);
            }

            position.x += position.width + 2;

            if(GUI.Button(position, "S")) {
                LocalizerEditorWindow.Open(key.stringValue);
            }

            if(dropdown) {
                var value = LocalizationSystem.GetLocalizedValue(key.stringValue);
                var style = GUI.skin.box;
                height = style.CalcHeight(new GUIContent(value), valueRect.width);

                valueRect.height = height;
                valueRect.y += 21;
                EditorGUI.LabelField(valueRect, value, EditorStyles.wordWrappedLabel);
            }

            if(!keyValue.Equals(string.Empty)) {
                var targetField = property.serializedObject.targetObject;
                fieldInfo.SetValue(targetField, new LocalizedString(keyValue));
                keyValue = string.Empty;
            }
            
            EditorGUI.EndProperty();
        }
    }
}