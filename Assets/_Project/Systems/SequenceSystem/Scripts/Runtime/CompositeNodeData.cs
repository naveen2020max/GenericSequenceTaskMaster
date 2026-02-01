using System.Collections.Generic;
using UnityEngine;

namespace SequenceSystem.Runtime
{
    [CreateAssetMenu(menuName = "Sequence/Nodes/Composite")]
    public class CompositeNodeData : BaseNodeData
    {
        [SerializeField] private bool _runInParallel;
        [SerializeField] private List<BaseNodeData> _subNodes;

        public bool RunInParallel => _runInParallel;
        public List<BaseNodeData> SubNodes => _subNodes;
    }
}