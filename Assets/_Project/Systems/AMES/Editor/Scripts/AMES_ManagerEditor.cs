using AMES.Runtime;
using UnityEditor;
using UnityEngine;

namespace AMES.Editor
{
    // This replaces the default Unity Inspector for the AMES_Manager
    [CustomEditor(typeof(AMES_Manager))]
    public class AMES_ManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the normal inspector (so we can still see the list)
            DrawDefaultInspector();

            AMES_Manager manager = (AMES_Manager)target;

            GUILayout.Space(15);

            // Draw a giant, unmissable button
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("BAKE SCENE AGENTS", GUILayout.Height(40)))
            {
                BakeAgents(manager);
            }
            GUI.backgroundColor = Color.white;
        }

        public static void BakeAgents(AMES_Manager manager)
        {
            // Find ALL agents in the scene, EVEN IF THEY ARE DISABLED!
            // Note: Using the bool 'true' tells Unity to include inactive objects.
            AMES_Agent[] allAgents = FindObjectsByType<AMES_Agent>(FindObjectsInactive.Include,FindObjectsSortMode.InstanceID);

            manager.PreBakedAgents.Clear();
            manager.PreBakedAgents.AddRange(allAgents);

            // Tell Unity we changed this object so it prompts the user to save the Scene
            EditorUtility.SetDirty(manager);

            Debug.Log($"[AMES] Successfully baked {allAgents.Length} agents into the Manager!");
        }
    }
}
