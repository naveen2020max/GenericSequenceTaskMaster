using UnityEngine;
using SequenceSystem.Domain;

namespace SequenceSystem.Runtime
{
    public abstract class BaseNodeData : ScriptableObject, INodeData
    {
        [Header("Identity")]
        [SerializeField] private string _guid;
        public string Guid => _guid;

        [Header("Timeout Settings")]
        [SerializeField] private bool _useTimeout;
        [SerializeField] private float _timeoutDuration = 10f;

        public bool UseTimeout => _useTimeout;
        public float TimeoutDuration => _timeoutDuration;

        [Header("Connections")]
        [SerializeField] private string _successGuid;
        [SerializeField] private string _failureGuid;
        [SerializeField] private string _timeoutGuid;

        public string SuccessGuid => _successGuid;
        public string FailureGuid => _failureGuid;
        public string TimeoutGuid => _timeoutGuid;

        // Note: For the Node Editor, we will generate the GUID in code.
        public void SetGuid(string newGuid) => _guid = newGuid;
    }
}