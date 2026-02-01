using System.Threading;
using Cysharp.Threading.Tasks;

namespace SequenceSystem.Domain
{
    public enum TaskStatus { Success, Failure, Timeout, Aborted }

    public interface IExecutionContext
    {
        RuntimeRegistry Registry { get; }
        Blackboard Blackboard { get; }
        CancellationToken CancellationToken { get; }
        ProcessorFactory Factory { get; }
    }

    public interface ITaskProcessor
    {
        UniTask<TaskStatus> ExecuteAsync(IExecutionContext context);
    }
}