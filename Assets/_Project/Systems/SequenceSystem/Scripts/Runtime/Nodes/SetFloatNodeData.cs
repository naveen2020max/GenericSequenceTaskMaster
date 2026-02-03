using UnityEngine;

namespace SequenceSystem.Runtime
{
    [CreateAssetMenu(menuName = "Sequence/Nodes/Actions/Set Float")]
    public class SetFloatNodeData : BaseNodeData
    {
        [BlackboardKey] // Uses your dropdown
        public string Key;

        public float Value;

        // Optional: Operation type (Set, Add, Subtract)
        public enum Op { Set, Add, Subtract }
        public Op Operation = Op.Set;
    }
}