using Cysharp.Threading.Tasks;
using SequenceSystem.Domain;
using System.Threading;
using UnityEngine;

namespace SequenceSystem.Runtime
{
    public class SequenceRunner : MonoBehaviour
    {
        [SerializeField] private SequenceGraphSO _graph;

        private SequenceExecutor _executor;
        private CancellationTokenSource _cts;

        public void StartSequence(ProcessorFactory factory, RuntimeRegistry registry, Blackboard blackboard)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            _executor = new SequenceExecutor(factory, registry, blackboard);
            _executor.RunGraph(_graph, _cts.Token).Forget();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
