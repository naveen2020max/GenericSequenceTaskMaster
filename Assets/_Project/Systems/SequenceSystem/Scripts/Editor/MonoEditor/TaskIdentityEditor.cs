using SequenceSystem.Runtime;
using UnityEditor;
using UnityEngine;

namespace SequenceSystem.Editor
{
    [CustomEditor(typeof(TaskIdentity))]
    public class TaskIdentityEditor : UnityEditor.Editor
    {
        private SerializedProperty _uniqueIdProp;
        private bool _isRegistried;

        private void OnEnable()
        {
            _uniqueIdProp = serializedObject.FindProperty("_uniqueId");
            AutoRegistryIfNotRegistered();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Register ID To RuntimeKeyDefinitions"))
            {
                AutoRegister();
            }
        }

        private void AutoRegistryIfNotRegistered()
        {
            if (!_isRegistried)
            {
                AutoRegister();
            }
        }

        private void AutoRegister()
        {
            serializedObject.Update();

            var task = (TaskIdentity)target;
            var definitions = FindRuntimeKeyDefinitions();
            if (definitions == null)
                return;

            string baseName = task.gameObject.name;
            string finalKey = ResolveDuplicate(baseName, definitions);

            // Rename GameObject if needed
            if (finalKey != baseName)
            {
                Undo.RecordObject(task.gameObject, "Rename TaskIdentity GameObject");
                task.gameObject.name = finalKey;
            }

            // Assign ID
            _uniqueIdProp.stringValue = finalKey;

            // Register key if missing
            if (!definitions.DefinedKeys.Contains(finalKey))
            {
                Undo.RecordObject(definitions, "Register Runtime Key");
                definitions.DefinedKeys.Add(finalKey);
                EditorUtility.SetDirty(definitions);
            }

            serializedObject.ApplyModifiedProperties();
            _isRegistried = true;
        }

        private string ResolveDuplicate(string baseName, RuntimeRegistryDefinitionsSO definitions)
        {
            if (!definitions.DefinedKeys.Contains(baseName))
                return baseName;

            int index = 1;
            string newName;

            do
            {
                newName = $"{baseName}_{index:00}";
                index++;
            }
            while (definitions.DefinedKeys.Contains(newName));

            return newName;
        }

        private RuntimeRegistryDefinitionsSO FindRuntimeKeyDefinitions()
        {
            var guids = AssetDatabase.FindAssets("t:RuntimeRegistryDefinitionsSO");
            if (guids.Length == 0)
                return null;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<RuntimeRegistryDefinitionsSO>(path);
        }
    }
}
