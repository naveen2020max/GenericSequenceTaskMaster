using SequenceSystem.Domain;
using UnityEngine;

namespace SequenceSystem.Runtime
{
    public class SequenceBootstrapper : MonoBehaviour
    {
        private RuntimeRegistry _registry;
        private Blackboard _globalBlackboard;

        private void Awake()
        {
            // 1. Create the Pure C# instances
            _registry = new RuntimeRegistry();
            _globalBlackboard = new Blackboard();

            // 2. Manual Injection: Find all identities in the scene
            // need to use a more efficient discovery method
            TaskIdentity[] identities = FindObjectsByType<TaskIdentity>(FindObjectsInactive.Include,FindObjectsSortMode.None);

            foreach (var identity in identities)
            {
                identity.Inject(_registry);
            }

            Debug.Log($"Sequence System Initialized. Registered {identities.Length} targets.");
        }

        // Accessors for the Executor to use later
        public RuntimeRegistry Registry => _registry;
        public Blackboard Blackboard => _globalBlackboard;
    }
}
