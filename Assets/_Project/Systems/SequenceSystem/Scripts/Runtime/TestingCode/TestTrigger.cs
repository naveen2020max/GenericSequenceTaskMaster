using SequenceSystem.Domain;
using UnityEngine;

namespace SequenceSystem.Runtime
{
    public class TestTrigger : MonoBehaviour
    {
        public SequenceRunner runner;
        public SequenceBootstrapper bootstrapper;

        void Start()
        {
            // Wait for bootstrapper to init
            Invoke(nameof(Run), 1f);
        }
        void Run()
        {
            runner.StartSequence(new ProcessorFactory(), bootstrapper.Registry, bootstrapper.Blackboard);
        }
    }
}
