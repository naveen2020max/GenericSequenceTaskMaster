using System;
using System.Collections.Generic;
using System.Linq;

namespace SequenceSystem.Domain
{
    public partial class Blackboard
    {
        private readonly Blackboard _parent;
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        public Blackboard(Blackboard parent = null)
        {
            _parent = parent;
        }

        public void Set(string key, object value)
        {
            if (HasKey(key))
            {
                _data[key] = value; 
            }
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            // Check local data first
            if (_data.TryGetValue(key, out var value))
            {
                return (T)value;
            }

            // If not found locally, check the parent (Recursive)
            if (_parent != null)
            {
                return _parent.Get<T>(key, defaultValue);
            }

            return defaultValue;
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (_data.TryGetValue(key, out var localValue))
            {
                value = (T)localValue;
                return true;
            }

            if (_parent != null)
            {
                return _parent.TryGet(key, out value);
            }

            value = default;
            return false;
        }

        public bool HasKey(string key)
        {
            if (_data.ContainsKey(key)) return true;
            return _parent != null && _parent.HasKey(key);
        }
    }

    [Serializable]
    public struct BlackboardEntry
    {
        public string Key;
        public string Value; // Stored as JSON or stringified
        public string TypeName;
    }

    public partial class Blackboard
    {
        // Export the current state for saving
        public BlackboardEntry[] SaveState()
        {
            return _data.Select(kvp => new BlackboardEntry
            {
                Key = kvp.Key,
                Value = kvp.Value.ToString(), // Simple stringification for now
                TypeName = kvp.Value.GetType().AssemblyQualifiedName
            }).ToArray();
        }

        // Restore state from a save file
        public void LoadState(BlackboardEntry[] entries)
        {
            _data.Clear();
            foreach (var entry in entries)
            {
                var type = Type.GetType(entry.TypeName);
                var val = Convert.ChangeType(entry.Value, type);
                _data[entry.Key] = val;
            }
        }
    }
}
