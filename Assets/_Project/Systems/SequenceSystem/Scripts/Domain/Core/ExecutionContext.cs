using System.Threading;

namespace SequenceSystem.Domain
{
    public class SequenceExecutionContext : IExecutionContext
    {
        public RuntimeRegistry Registry { get; }
        public Blackboard Blackboard { get; }
        public CancellationToken CancellationToken { get; }
        public ProcessorFactory Factory { get; }

        public SequenceExecutionContext(RuntimeRegistry registry, Blackboard blackboard, CancellationToken ct, ProcessorFactory factory)
        {
            Registry = registry;
            Blackboard = blackboard;
            CancellationToken = ct;
            Factory = factory;
        }
    }
}
