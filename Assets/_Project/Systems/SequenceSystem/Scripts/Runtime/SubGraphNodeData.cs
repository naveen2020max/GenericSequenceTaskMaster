using UnityEngine;

namespace SequenceSystem.Runtime
{
    [CreateAssetMenu(menuName = "Sequence/Nodes/SubGraph")]
    public class SubGraphNodeData : BaseNodeData
    {
        [SerializeField] private SequenceGraphSO _targetGraph;
        public SequenceGraphSO TargetGraph => _targetGraph;
    }
}
