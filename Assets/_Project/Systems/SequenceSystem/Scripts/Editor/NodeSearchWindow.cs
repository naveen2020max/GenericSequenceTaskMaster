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
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private SequenceGraphView _graphView;
        private SequenceGraphWindow _window;

        public void Initialize(SequenceGraphWindow window, SequenceGraphView graphView)
        {
            _window = window;
            _graphView = graphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0)
            };

            // 1. Reflection: Find all non-abstract types inheriting from BaseNodeData
            var nodeTypes = TypeCache.GetTypesDerivedFrom<BaseNodeData>()
                .Where(t => !t.IsAbstract && !t.IsGenericType)
                .OrderBy(t => t.Name)
                .ToList();

            // 2. Build the Tree
            foreach (var type in nodeTypes)
            {
                // You could use attributes here to create categories (e.g. "Action/Move")
                // For now, we just list them flat
                string menuName = type.Name.Replace("NodeData", ""); // Clean up name
                tree.Add(new SearchTreeEntry(new GUIContent(menuName))
                {
                    userData = type,
                    level = 1
                });
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var type = (Type)SearchTreeEntry.userData;

            // Calculate position
            var worldMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(
                _window.rootVisualElement.parent, context.screenMousePosition - _window.position.position);
            var localMousePosition = _graphView.contentViewContainer.WorldToLocal(worldMousePosition);

            // Command the GraphView to create the node
            _graphView.CreateNode(type, localMousePosition);
            return true;
        }

        
    }
}