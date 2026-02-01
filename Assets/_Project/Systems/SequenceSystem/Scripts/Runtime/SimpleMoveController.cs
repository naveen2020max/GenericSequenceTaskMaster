using SequenceSystem.Domain;
using UnityEngine;

namespace SequenceSystem.Runtime
{
    // Implements IMovable so the Processor can control it
    // Implements IStateSync so it resets correctly on hard reload
    public class SimpleMoveController : MonoBehaviour, IMovable, IStateSync
    {
        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        // Called automatically when the scene hard-resets
        public void SyncWithBlackboard(Blackboard blackboard)
        {
            // If we saved the position in the blackboard, restore it here.
            // For this simple example, we won't implement complex persistence yet.
        }
    }
}
