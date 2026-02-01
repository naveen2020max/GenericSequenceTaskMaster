using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SequenceSystem.Domain;

namespace SequenceSystem.Runtime
{
    public class SequenceExecutor
    {
        private readonly ProcessorFactory _factory;
        private readonly RuntimeRegistry _registry;
        private readonly Blackboard _blackboard;

        public SequenceExecutor(ProcessorFactory factory, RuntimeRegistry registry, Blackboard blackboard)
        {
            _factory = factory;
            _registry = registry;
            _blackboard = blackboard;
        }

        public async UniTask RunGraph(SequenceGraphSO graph, CancellationToken externalToken)
        {
            string currentGuid = graph.EntryNodeGuid;

            while (!string.IsNullOrEmpty(currentGuid))
            {
                // 1. Find Node Data
                var nodeData = graph.GetNodeByGuid(currentGuid);
                if (nodeData == null) break;

                // 2. Create Processor
                var processor = _factory.CreateProcessor(nodeData);

                // 3. Setup Linked Cancellation and Timeout
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
                if (nodeData.UseTimeout) cts.CancelAfterSlim(TimeSpan.FromSeconds(nodeData.TimeoutDuration));

                var context = new SequenceExecutionContext(_registry, _blackboard, cts.Token, _factory);

                // 4. Execute and handle result
                TaskStatus result;
                try
                {
                    result = await processor.ExecuteAsync(context);
                }
                catch (OperationCanceledException)
                {
                    result = cts.IsCancellationRequested ? TaskStatus.Timeout : TaskStatus.Aborted;
                }

                // 5. Determine Next Node (Traversal)
                string nextGuid = "";

                switch (result)
                {
                    case TaskStatus.Success:
                        nextGuid = nodeData.SuccessGuid;
                        break;
                    case TaskStatus.Failure:
                        nextGuid = nodeData.FailureGuid;
                        break;
                    case TaskStatus.Timeout:
                        nextGuid = nodeData.TimeoutGuid;
                        break;
                    case TaskStatus.Aborted:
                        nextGuid = null; // Stop execution
                        break;
                }

                currentGuid = nextGuid;

                // 6. Mandatory 1-frame delay
                await UniTask.Yield();
            }
        }
    }
}