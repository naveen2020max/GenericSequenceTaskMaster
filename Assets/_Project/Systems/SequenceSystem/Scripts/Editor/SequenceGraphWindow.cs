// File: SequenceGraphWindow.cs (Editor Assembly)
using SequenceSystem.Runtime;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace SequenceSystem.Editor
{
    public class SequenceGraphWindow : EditorWindow
    {
        private SequenceGraphView _graphView;
        private SequenceGraphSO _currentGraph;

        private Label _fileNameLabel; // Reference to update the UI text

        [MenuItem("Sequence System/Graph Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<SequenceGraphWindow>();
            window.titleContent = new GUIContent("Sequence Editor");
            window.Show();
        }

        // Call this to open a specific asset
        public static void OpenGraph(SequenceGraphSO graph)
        {
            //var window = GetWindow<SequenceGraphWindow>();
            //window.LoadGraph(graph);
            //window.Show();

            if (graph == null) return;

            var window = CreateInstance<SequenceGraphWindow>();
            window.titleContent = new GUIContent($"Sequence: {graph.name}");
            window.LoadGraph(graph);
            window._currentGraph = graph;
            window.Show();
        }

        // 1. Double Click Support: Opens window when you double-click asset
        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var obj = EditorUtility.EntityIdToObject(instanceID);
            if (obj is SequenceGraphSO graph)
            {
                
                OpenGraph(graph);
                //var window = GetWindow<SequenceGraphWindow>();
                //window.LoadGraph(graph);
                return true;
            }
            return false;
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
            if (_currentGraph) { LoadGraph(_currentGraph); Show(); }
            // 2. Selection Change Support: Listen to project clicks
            //Selection.selectionChanged += OnSelectionChanged;

            //// Try to load whatever is currently selected right now
            //OnSelectionChanged();
        }

        private void OnDisable()
        {
            if (_graphView != null)
            {
                rootVisualElement.Remove(_graphView);
            }
        }

        // 3. The Auto-Load Logic
        //private void OnSelectionChanged()
        //{
        //    var obj = Selection.activeObject;
        //    if (obj is SequenceGraphSO graph)
        //    {
        //        LoadGraph(graph);
        //    }
        //}

        private void ConstructGraphView()
        {
            _graphView = new SequenceGraphView(this)
            {
                name = "Sequence Graph"
            };

            // Make the graph take up the whole window
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void GenerateToolbar()
        {
            // A simple toolbar to show the name of the asset we are editing
            var toolbar = new UnityEditor.UIElements.Toolbar();

            _fileNameLabel = new Label("No Graph Selected");
            _fileNameLabel.name = "FileNameLabel"; // We can reference this later to update text
            _fileNameLabel.style.marginLeft = 10;
            _fileNameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            var graphlabel = new Label($"{_currentGraph}");

            var loadgraphbtn = new Button(()=> { LoadGraph(_currentGraph); Show(); });
            loadgraphbtn.text = "LoadGraph";

            toolbar.Add(_fileNameLabel);
            toolbar.Add(graphlabel);
            toolbar.Add(loadgraphbtn);
            rootVisualElement.Add(toolbar);
        }

        public void LoadGraph(SequenceGraphSO graph)
        {
            if (graph == null) return;
            if (_currentGraph == graph) return; // Don't reload if already open
            _currentGraph = graph;

            // Update Toolbar Text
            if (_fileNameLabel != null)
                _fileNameLabel.text = $"Editing: {_currentGraph.name}";

            // 1. Ensure Layout Asset exists
            VerifyLayoutAsset(graph);

            // 2. Tell GraphView to populate
            _graphView.PopulateView(graph);
        }

        private void VerifyLayoutAsset(SequenceGraphSO graph)
        {
            if (graph.EditorLayout == null)
            {
                // Logic to automatically create the layout asset if it's missing
                // This keeps the "Two Asset" system seamless for the user
                string graphPath = AssetDatabase.GetAssetPath(graph);
                string layoutPath = graphPath.Replace(".asset", "_Layout.asset");

                var layout = AssetDatabase.LoadAssetAtPath<SequenceGraphLayoutSO>(layoutPath);

                if (layout == null)
                {
                    layout = CreateInstance<SequenceGraphLayoutSO>();
                    AssetDatabase.CreateAsset(layout, layoutPath);
                }

                graph.EditorLayout = layout;
                EditorUtility.SetDirty(graph);
                AssetDatabase.SaveAssets();
            }
        }
    }
}