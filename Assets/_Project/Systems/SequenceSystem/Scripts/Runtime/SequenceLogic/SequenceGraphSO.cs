using System.Collections.Generic;
using UnityEngine;
using SequenceSystem.Domain;

namespace SequenceSystem.Runtime
{
    [CreateAssetMenu(menuName = "Sequence/Graph")]
    public class SequenceGraphSO : ScriptableObject, IGraphData
    {
        [SerializeField] private string _entryNodeGuid;
        [SerializeField] private List<BaseNodeData> _allNodes = new List<BaseNodeData>();

        // Lookup cache for performance
        private Dictionary<string, INodeData> _nodeCache;

        public string EntryNodeGuid => _entryNodeGuid;

        public List<BaseNodeData> GetAllNodes() => _allNodes;

        public INodeData GetNodeByGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;

            // Initialize cache if needed
            if (_nodeCache == null || _nodeCache.Count != _allNodes.Count)
            {
                _nodeCache = new Dictionary<string, INodeData>();
                foreach (var node in _allNodes)
                {
                    _nodeCache[node.Guid] = node;
                }
            }

            _nodeCache.TryGetValue(guid, out var result);
            return result;
        }

        // Editor helper
        public void AddNode(BaseNodeData node) => _allNodes.Add(node);

        public void RemoveNode(BaseNodeData node)
        {
            if (_allNodes.Contains(node))
            {
                _allNodes.Remove(node);
            }
        }

#if UNITY_EDITOR
        // This field only exists in the Editor. The build won't see it.
        [SerializeField] private SequenceGraphLayoutSO _editorLayout;

        public SequenceGraphLayoutSO EditorLayout
        {
            get => _editorLayout;
            set => _editorLayout = value;
        }
#endif
    }
}