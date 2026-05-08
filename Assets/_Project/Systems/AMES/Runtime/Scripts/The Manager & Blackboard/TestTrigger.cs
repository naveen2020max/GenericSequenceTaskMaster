using UnityEngine;
using UnityEngine.InputSystem;

namespace AMES.Runtime
{
    public class TestTrigger : MonoBehaviour
    {
        public AMES_EventNode TestEvent; // Drag your 'Event_01_BridgeExplodes' SO here in the inspector!

        void Update()
        {
            if (Keyboard.current[Key.Space].wasPressedThisFrame)
            {
                // Execute the event when we press Spacebar
                AMES_Manager.Instance.ExecuteEvent(TestEvent);
            }
        }
    }
}
