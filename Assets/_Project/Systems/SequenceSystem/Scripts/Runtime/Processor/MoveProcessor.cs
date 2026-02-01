using Cysharp.Threading.Tasks;
using SequenceSystem.Domain;
using UnityEngine;

namespace SequenceSystem.Runtime
{
    [Processor(typeof(MoveNodeData))]
    public class MoveProcessor : ITaskProcessor
    {
        private readonly MoveNodeData _data;

        public MoveProcessor(MoveNodeData data)
        {
            _data = data;
        }

        public async UniTask<TaskStatus> ExecuteAsync(IExecutionContext context)
        {
            // 1. Resolve the Target (Async to handle initialization race conditions)
            var target = await context.Registry.ResolveAsync<IMovable>(_data.TargetID);

            if (target == null)
            {
                Debug.LogError($"[MoveProcessor] Could not find target with ID: {_data.TargetID}");
                return TaskStatus.Failure;
            }

            // 2. The Movement Loop
            while (Vector3.Distance(target.Position, _data.TargetPosition) > _data.Tolerance)
            {
                // Check for cancellation (STOP if the game resets or times out)
                if (context.CancellationToken.IsCancellationRequested)
                    return TaskStatus.Aborted;

                // Move towards target
                // Note: Using Unity's Time.deltaTime requires UnityEngine access
                float step = _data.Speed * Time.deltaTime;
                target.Position = Vector3.MoveTowards(target.Position, _data.TargetPosition, step);

                // Wait for next frame
                await UniTask.Yield();
            }

            // 3. Snap to final position to be precise
            target.Position = _data.TargetPosition;

            return TaskStatus.Success;
        }
    }
}
