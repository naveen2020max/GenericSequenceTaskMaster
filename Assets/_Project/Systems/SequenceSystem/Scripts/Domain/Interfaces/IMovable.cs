using UnityEngine;

namespace SequenceSystem.Domain
{
    // Inherits ITaskTarget so it can be stored in the Registry
    public interface IMovable : ITaskTarget
    {
        Vector3 Position { get; set; }
    }
}
