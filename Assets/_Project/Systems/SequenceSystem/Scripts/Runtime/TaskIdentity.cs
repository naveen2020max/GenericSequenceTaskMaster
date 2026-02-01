using SequenceSystem.Domain;
using UnityEngine;

namespace SequenceSystem.Runtime
{
    public class TaskIdentity : MonoBehaviour
    {
        [SerializeField] private string _uniqueId;
        private RuntimeRegistry _registry;

        // This is the Injection point
        public void Inject(RuntimeRegistry registry)
        {
            _registry = registry;

            // Immediately register ourselves
            var target = GetComponent<ITaskTarget>();
            _registry.Register(_uniqueId, target);
        }

        private void OnDestroy()
        {
            // Clean up to prevent memory leaks
            _registry?.Unregister(_uniqueId);
        }
    }
}
