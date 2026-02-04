using UnityEngine;

namespace SequenceSystem.Runtime
{
    [CreateAssetMenu(menuName = "Sequence/Nodes/Actions/Move")]
    public class MoveNodeData : BaseNodeData
    {
        [Header("Settings")]
        [RuntimeRegistryKey]public string TargetID;        // Which object to move (e.g., "Player")
        public Vector3 TargetPosition; // Where to go
        public float Speed = 5f;       // How fast
        public float Tolerance = 0.1f; // How close is "close enough"
    }
}
