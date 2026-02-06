using SequenceSystem.Runtime;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TaskIdentity))]
public class TaskIdentityEditor : Editor
{
    private SerializedProperty _uniqueIdProp;

    private void OnEnable()
    {
        _uniqueIdProp = serializedObject.FindProperty("_uniqueId");
        AutoRegister();
    }
    private void OnDisable()
    {
        // If target is null → component or GameObject was deleted
        if (target == null)
        {
            CleanupRegistryKey();
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }

    private void AutoRegister()
    {
        serializedObject.Update();

        var task = (TaskIdentity)target;
        var definitions = FindRuntimeKeyDefinitions();
        if (definitions == null)
            return;

        string baseName = task.gameObject.name;
        string finalKey = baseName;

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
    }

    private string ResolveDuplicate(string baseName, RuntimeRegistryDefinitionsSO definitions)
    {
        if (!definitions.DefinedKeys.Contains(baseName) )
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

    private void CleanupRegistryKey()
    {
        var definitions = FindRuntimeKeyDefinitions();
        if (definitions == null)
            return;

        // SerializedObject is already invalid, so read last cached value
        string key = _uniqueIdProp?.stringValue;

        if (string.IsNullOrEmpty(key))
            return;

        if (definitions.DefinedKeys.Contains(key))
        {
            Undo.RecordObject(definitions, "Unregister Runtime Key");
            definitions.DefinedKeys.Remove(key);
            EditorUtility.SetDirty(definitions);
            AssetDatabase.SaveAssets();
        }
    }
}
