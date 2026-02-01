using Cysharp.Threading.Tasks;
using SequenceSystem.Domain;
using UnityEngine;

namespace SequenceSystem.Runtime
{
    [Processor(typeof(ComparisonNodeData))]
    public class ComparisonProcessor : ITaskProcessor
    {
        private readonly ComparisonNodeData _data;

        public ComparisonProcessor(ComparisonNodeData data) => _data = data;

        public async UniTask<TaskStatus> ExecuteAsync(IExecutionContext context)
        {
            // 1. Get value from blackboard
            float currentVal = context.Blackboard.Get<float>(_data.BlackboardKey);

            // 2. Evaluate
            bool success = _data.Comparison switch
            {
                ComparisonType.Equal => Mathf.Approximately(currentVal, _data.ValueToCompare),
                ComparisonType.NotEqual => !Mathf.Approximately(currentVal, _data.ValueToCompare),
                ComparisonType.Greater => currentVal > _data.ValueToCompare,
                ComparisonType.Less => currentVal < _data.ValueToCompare,
                ComparisonType.GreaterOrEqual => currentVal >= _data.ValueToCompare,
                ComparisonType.LessOrEqual => currentVal <= _data.ValueToCompare,
                _ => false
            };

            return success ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
