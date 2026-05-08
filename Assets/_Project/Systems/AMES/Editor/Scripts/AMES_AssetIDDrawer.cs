using AMES.Runtime;
using UnityEditor;
using UnityEngine;

namespace AMES.Editor
{
    // This tells Unity: "Whenever you see [AMES_AssetID], run this code to draw the Inspector UI."
    [CustomPropertyDrawer(typeof(AMES_AssetIDAttribute))]
    public class AMES_AssetIDDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 1. Find the Master Database in the project
            string[] guids = AssetDatabase.FindAssets("t:AMES_MasterDatabase");

            if (guids.Length == 0)
            {
                // If the designer hasn't created the database yet, show a warning text box
                EditorGUI.PropertyField(position, property, new GUIContent(label.text + " (DB Missing!)"));
                return;
            }

            // 2. Load the Database
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            AMES_MasterDatabase db = AssetDatabase.LoadAssetAtPath<AMES_MasterDatabase>(path);

            if (db.RegisteredAssetIDs == null || db.RegisteredAssetIDs.Count == 0)
            {
                EditorGUI.LabelField(position, label.text, "Database is empty!");
                return;
            }

            // 3. Get the list of IDs
            string[] options = db.GetAssetIDArray();

            // 4. Find which ID is currently selected
            int selectedIndex = System.Array.IndexOf(options, property.stringValue);
            if (selectedIndex == -1) selectedIndex = 0; // Default to the first one if not found

            // 5. Draw the Magic Dropdown!
            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, options);

            // 6. Save the selected string back to the variable
            property.stringValue = options[selectedIndex];
        }
    }
}
