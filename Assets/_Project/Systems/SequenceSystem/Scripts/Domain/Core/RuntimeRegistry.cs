using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace SequenceSystem.Domain
{
    public class RuntimeRegistry
    {
        private readonly Dictionary<string, ITaskTarget> _targets = new Dictionary<string, ITaskTarget>();

        public void Register(string id, ITaskTarget target)
        {
            if (string.IsNullOrEmpty(id) || target == null) return;
            _targets[id] = target;
        }

        public void Unregister(string id)
        {
            if (_targets.ContainsKey(id)) _targets.Remove(id);
        }

        public T GetAs<T>(string id) where T : class, ITaskTarget
        {
            if (_targets.TryGetValue(id, out var target))
            {
                return target as T;
            }
            return null;
        }

        public async UniTask<T> ResolveAsync<T>(string id, int timeoutFrames = 60) where T : class, ITaskTarget
        {
            int frames = 0;
            while (!_targets.ContainsKey(id) && frames < timeoutFrames)
            {
                await UniTask.Yield();
                frames++;
            }

            return GetAs<T>(id);
        }
    }
}
