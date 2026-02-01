
using SequenceSystem.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SequenceSystem.Editor
{
    public class SequenceGraphView : GraphView
    {
        private SequenceGraphWindow _window;
        private SequenceGraphSO _graph;

        // Cache to quickly find views by GUID
        private Dictionary<string, SequenceNodeView> _nodeViewMap = new Dictionary<string, SequenceNodeView>();
        private NodeSearchWindow _searchWindow;

        public SequenceGraphView(SequenceGraphWindow window)
        {
            _window = window;

            // 1. Standard GraphView Setup
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            // 2. Add the Grid Background
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            // 3. Zoom
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // Setup Search Window
            _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _searchWindow.Initialize(window, this);

            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        public void PopulateView(SequenceGraphSO graph)
        {
            _graph = graph;

            // Clear existing nodes (if reloading)
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            // TODO: Create Node Views from Graph Data
            // 2. Create Node Views
            // We assume the graph has a list of nodes. 
            // Note: We need to expose the list in SequenceGraphSO or use the public AddNode method later.
            // For now, let's assume we can access them (you might need to make the list public or add a getter).
            foreach (var nodeData in _graph.GetAllNodes())
            {
                CreateNodeView(nodeData);
            }

            // 3. Create Edges (Connections)
            foreach (var nodeData in _graph.GetAllNodes())
            {
                var sourceView = _nodeViewMap[nodeData.Guid];

                // Connect Success
                ConnectPorts(sourceView.SuccessPort, nodeData.SuccessGuid);

                // Connect Failure
                ConnectPorts(sourceView.FailurePort, nodeData.FailureGuid);

                // Connect Timeout
                if (nodeData.UseTimeout)
                {
                    ConnectPorts(sourceView.TimeoutPort, nodeData.TimeoutGuid);
                }
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            // TODO: Handle deletions and movements
            if (_graph == null) return graphViewChange;

            // Handle Moves
            if (graphViewChange.movedElements != null)
            {
                foreach (var element in graphViewChange.movedElements)
                {
                    if (element is SequenceNodeView nodeView)
                    {
                        var pos = nodeView.GetPosition().position;
                        _graph.EditorLayout.UpdatePosition(nodeView.NodeData.Guid, pos);
                    }
                }
                EditorUtility.SetDirty(_graph.EditorLayout);
            }

            // Handle Connections (Edges Created)
            if (graphViewChange.edgesToCreate != null)
            {
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    var parentNode = edge.output.node as SequenceNodeView;
                    var childNode = edge.input.node as SequenceNodeView;

                    // Use SerializedObject to safely write the GUID
                    SerializedObject so = new SerializedObject(parentNode.NodeData);
                    string portName = edge.output.portName; // "Success", "Failure", "Timeout"

                    // Map Port Name to Field Name (Pure Option B)
                    string propertyName = portName switch
                    {
                        "Success" => "_successGuid",
                        "Failure" => "_failureGuid",
                        "Timeout" => "_timeoutGuid",
                        _ => ""
                    };

                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        so.FindProperty(propertyName).stringValue = childNode.NodeData.Guid;
                        so.ApplyModifiedProperties();
                    }
                }
            }

            // Handle Deletions (Nodes & Edges)
            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    if (element is SequenceNodeView nodeView)
                    {
                        // Remove from Graph List
                        // Note: You need a RemoveNode method in SequenceGraphSO
                        _graph.RemoveNode(nodeView.NodeData);

                        // Destroy Sub-Asset
                        Undo.DestroyObjectImmediate(nodeView.NodeData);

                        EditorUtility.SetDirty(_graph);
                    }
                    else if (element is Edge edge)
                    {
                        // Disconnect Logic
                        var parentNode = edge.output.node as SequenceNodeView;
                        SerializedObject so = new SerializedObject(parentNode.NodeData);

                        string portName = edge.output.portName;
                        string propertyName = portName switch
                        {
                            "Success" => "_successGuid",
                            "Failure" => "_failureGuid",
                            "Timeout" => "_timeoutGuid",
                            _ => ""
                        };

                        if (!string.IsNullOrEmpty(propertyName))
                        {
                            so.FindProperty(propertyName).stringValue = ""; // Clear GUID
                            so.ApplyModifiedProperties();
                        }
                    }
                }
            }
            return graphViewChange;
        }

        public void CreateNodeView(BaseNodeData nodeData)
        {
            var nodeView = new SequenceNodeView(nodeData);

            // Load position from the Layout Asset
            var pos = _graph.EditorLayout.GetPosition(nodeData.Guid);
            nodeView.SetPosition(new Rect(pos, Vector2.zero));

            _nodeViewMap[nodeData.Guid] = nodeView;
            AddElement(nodeView);
        }

        private void ConnectPorts(Port outputPort, string targetGuid)
        {
            if (string.IsNullOrEmpty(targetGuid)) return;

            if (_nodeViewMap.TryGetValue(targetGuid, out var targetView))
            {
                var edge = outputPort.ConnectTo(targetView.InputPort);
                AddElement(edge);
            }
        }
        // This is required for the GraphView to allow connections
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node).ToList();
        }

        // 1. CREATION LOGIC
        public void CreateNode(Type type, Vector2 position)
        {
            // A. Create the SO
            var node = ScriptableObject.CreateInstance(type) as BaseNodeData;
            node.name = type.Name;
            node.SetGuid(Guid.NewGuid().ToString());

            // B. Add as Sub-Asset (Asset Management)
            AssetDatabase.AddObjectToAsset(node, _graph);

            // C. Add to List and Layout
            _graph.AddNode(node); // Ensure AddNode is public in SequenceGraphSO
            _graph.EditorLayout.UpdatePosition(node.Guid, position);

            // D. Save
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(_graph);

            // E. Refresh View
            CreateNodeView(node);
        }
    }
}