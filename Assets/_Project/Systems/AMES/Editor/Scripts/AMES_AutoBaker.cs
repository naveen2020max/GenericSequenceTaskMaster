using AMES.Runtime;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AMES.Editor
{
    // 1. Auto-Bake when entering Play Mode
    [InitializeOnLoad]
    public static class AMES_PlayModeAutoBaker
    {
        static AMES_PlayModeAutoBaker()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                AutoBakeCurrentScene();
            }
        }

        public static void AutoBakeCurrentScene()
        {
            AMES_Manager manager = Object.FindFirstObjectByType<AMES_Manager>();
            if (manager != null)
            {
                AMES_ManagerEditor.BakeAgents(manager);
            }
        }
    }

    // 2. Auto-Bake when Building the Game (Creating the .exe / .apk)
    public class AMES_BuildAutoBaker : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(UnityEngine.SceneManagement.Scene scene, BuildReport report)
        {
            // This runs for every scene right before it gets packed into the final game build!
            AMES_PlayModeAutoBaker.AutoBakeCurrentScene();
        }
    }
}
