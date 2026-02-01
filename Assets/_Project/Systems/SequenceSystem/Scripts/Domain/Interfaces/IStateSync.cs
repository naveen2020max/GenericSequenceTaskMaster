

namespace SequenceSystem.Domain
{
    // Interface for objects that need to restore state after a scene reset
    public interface IStateSync
    {
        void SyncWithBlackboard(Blackboard blackboard);
    }
}
