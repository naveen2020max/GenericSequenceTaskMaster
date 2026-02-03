using System;
using System.Collections.Generic;
using UnityEngine;

namespace SequenceSystem.Runtime
{
    [Serializable]
    public struct NodeLayoutData
    {
        public string NodeGuid;
        public Vector2 Position;
        // We can add "Group" or "Comment" data here later
    }

    // This asset runs side-by-side with the Logic Graph to tell the Editor where to draw things.
    public class SequenceGraphLayoutSO : ScriptableObject
    {
        public List<NodeLayoutData> NodePositions = new List<NodeLayoutData>();

        public void UpdatePosition(string guid, Vector2 position)
        {
            // Simple linear search is fine for editor usage
            int index = NodePositions.FindIndex(x => x.NodeGuid == guid);
            if (index != -1)
            {
                var data = NodePositions[index];
                data.Position = position;
                NodePositions[index] = data;
            }
            else
            {
                NodePositions.Add(new NodeLayoutData { NodeGuid = guid, Position = position });
            }
        }

        public Vector2 GetPosition(string guid)
        {
            var data = NodePositions.Find(x => x.NodeGuid == guid);
            // Default to (0,0) if not found, or some offset
            return data.NodeGuid != null ? data.Position : Vector2.zero;
        }
    }
}
