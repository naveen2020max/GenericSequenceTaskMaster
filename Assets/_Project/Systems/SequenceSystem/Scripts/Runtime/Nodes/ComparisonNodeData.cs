using UnityEngine;

namespace SequenceSystem.Runtime
{
    public enum ComparisonType { Equal, NotEqual, Greater, Less, GreaterOrEqual, LessOrEqual }

    [CreateAssetMenu(menuName = "Sequence/Nodes/Comparison")]
    public class ComparisonNodeData : BaseNodeData
    {
        [SerializeField] [BlackboardKey] private string _blackboardKey;
        [SerializeField] private ComparisonType _comparison;
        [SerializeField] private float _valueToCompare; // For simplicity, we'll start with floats

        public string BlackboardKey => _blackboardKey;
        public ComparisonType Comparison => _comparison;
        public float ValueToCompare => _valueToCompare;
    }
}
