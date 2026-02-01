using Cysharp.Threading.Tasks;
using SequenceSystem.Domain;

namespace SequenceSystem.Runtime
{
    [Processor(typeof(SubGraphNodeData))]
    public class SubGraphProcessor : ITaskProcessor
    {
        private readonly SubGraphNodeData _data;

        public SubGraphProcessor(SubGraphNodeData data) => _data = data;

        public async UniTask<TaskStatus> ExecuteAsync(IExecutionContext context)
        {
            if (_data.TargetGraph == null) return TaskStatus.Failure;

            // CREATE LOCAL SCOPE: Pass current blackboard as parent to a new local one
            var localBlackboard = new Blackboard(context.Blackboard);

            // Create a new Executor for the child graph
            var subExecutor = new SequenceExecutor(context.Factory, context.Registry, localBlackboard);

            // Await the sub-graph execution
            await subExecutor.RunGraph(_data.TargetGraph, context.CancellationToken);

            return TaskStatus.Success;
        }
    }
}
