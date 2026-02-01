namespace SequenceSystem.Domain
{
    public interface IGraphData
    {
        string EntryNodeGuid { get; }
        INodeData GetNodeByGuid(string guid);
    }
}
