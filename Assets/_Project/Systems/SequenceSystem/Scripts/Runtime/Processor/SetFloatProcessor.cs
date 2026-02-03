using Cysharp.Threading.Tasks;
using SequenceSystem.Domain;

namespace SequenceSystem.Runtime
{
    [Processor(typeof(SetFloatNodeData))]
    public class SetFloatProcessor : ITaskProcessor
    {
        private readonly SetFloatNodeData _data;

        public SetFloatProcessor(SetFloatNodeData data) => _data = data;

        public async UniTask<TaskStatus> ExecuteAsync(IExecutionContext context)
        {
            float current = 0;
            context.Blackboard.TryGet<float>(_data.Key, out current);
            float finalValue = 0f;

            switch (_data.Operation)
            {
                case SetFloatNodeData.Op.Set:
                    finalValue = _data.Value;
                    break;
                case SetFloatNodeData.Op.Add:
                    finalValue = current + _data.Value;
                    break;
                case SetFloatNodeData.Op.Subtract:
                    finalValue = current - _data.Value;
                    break;
            }

            // Write to Blackboard
            context.Blackboard.Set(_data.Key, finalValue);

            return TaskStatus.Success;
        }
    }
}