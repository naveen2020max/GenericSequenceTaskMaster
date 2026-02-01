using Cysharp.Threading.Tasks;
using SequenceSystem.Domain;
using System.Linq;
using System.Threading;

// all processor should be in domain assembly but because of some complecation moving it to runtime 
// maybe change it in future 2 issues the nodedata it need that data which is in runtime because it is scriptableObject
// second if a processor need to do any changes in scene it need reference which it can only get from runtime assembly so 
// some assembly will be forced to move to runtime need to think
namespace SequenceSystem.Runtime
{
    [Processor(typeof(CompositeNodeData))]
    public class CompositeProcessor : ITaskProcessor
    {
        private readonly CompositeNodeData _data;

        public CompositeProcessor(CompositeNodeData data) => _data = data;

        public async UniTask<TaskStatus> ExecuteAsync(IExecutionContext context)
        {
            if (_data.SubNodes == null || _data.SubNodes.Count == 0) return TaskStatus.Success;

            if (_data.RunInParallel)
                return await ExecuteParallel(context);

            return await ExecuteSequential(context);
        }

        private async UniTask<TaskStatus> ExecuteSequential(IExecutionContext context)
        {
            foreach (var node in _data.SubNodes)
            {
                var processor = context.Factory.CreateProcessor(node);
                var status = await processor.ExecuteAsync(context);

                if (status != TaskStatus.Success) return status;

                await UniTask.Yield(); // 1-frame delay per sub-task
            }
            return TaskStatus.Success;
        }

        private async UniTask<TaskStatus> ExecuteParallel(IExecutionContext context)
        {
            using var parallelCts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
            var internalContext = new SequenceExecutionContext(context.Registry, context.Blackboard, parallelCts.Token, context.Factory);

            var tasks = _data.SubNodes.Select(async node =>
            {
                var proc = context.Factory.CreateProcessor(node);
                var status = await proc.ExecuteAsync(internalContext);

                if (status != TaskStatus.Success)
                {
                    parallelCts.Cancel(); // FAIL-FAST: Kill siblings
                    return status;
                }

                await UniTask.Yield(); // 1-frame delay for consistency
                return status;
            }).ToList();

            var results = await UniTask.WhenAll(tasks);

            // If any aren't success, the whole node failed
            return results.All(r => r == TaskStatus.Success) ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
