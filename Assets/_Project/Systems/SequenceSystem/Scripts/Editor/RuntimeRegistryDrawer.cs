using SequenceSystem.Runtime;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SequenceSystem.Editor
{
    [CustomPropertyDrawer(typeof(RuntimeRegistryKeyAttribute))]
    public class RuntimeRegistryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 1. Find the Definitions Asset
            // We search the AssetDatabase to find the config file automatically.
            var guids = AssetDatabase.FindAssets("t:RuntimeRegistryDefinitionsSO");

            if (guids.Length == 0)
            {
                // Fallback: If no config exists, just draw a normal text field with a warning
                EditorGUI.PropertyField(position, property, label);
                Debug.LogWarning("No 'RuntimeRegistryDefinitionsSO' found. Create one to use the Dropdown.");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var defs = AssetDatabase.LoadAssetAtPath<RuntimeRegistryDefinitionsSO>(path);

            if (defs == null || defs.DefinedKeys.Count == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            // 2. Prepare the List
            List<string> options = new List<string>(defs.DefinedKeys);
            options.Insert(0, "None / Custom"); // Allow an option to type manually if needed

            // 3. Find current index
            int index = options.IndexOf(property.stringValue);
            if (index == -1) index = 0; // Default to "Custom" if current value isn't in list

            // 4. Draw Dropdown
            // Calculate rects
            Rect dropdownRect = new Rect(position.x, position.y, position.width, position.height);

            EditorGUI.BeginProperty(position, label, property);

            int newIndex = EditorGUI.Popup(dropdownRect, label.text, index, options.ToArray());

            // 5. Apply Selection
            if (newIndex > 0)
            {
                property.stringValue = options[newIndex];
            }
            else if (newIndex == 0 && index != 0)
            {
                // If they switched back to "Custom", clear it or leave it? 
                // Usually leaving it is safer so they don't lose data.
            }

            EditorGUI.EndProperty();
        }
    }
}
