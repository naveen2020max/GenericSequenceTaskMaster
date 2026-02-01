namespace SequenceSystem.Domain
{
    public interface INodeData
    {
        string Guid { get; }
        bool UseTimeout { get; }
        float TimeoutDuration { get; }

        // Explicit GUID links for the three main states
        string SuccessGuid { get; }
        string FailureGuid { get; }
        string TimeoutGuid { get; }
    }
}
