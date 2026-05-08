using UnityEngine;
using static AMES.Runtime.AMES_DataTypes;

namespace AMES.Runtime
{
    public class AMES_Agent : MonoBehaviour
    {
        [AMES_AssetID] // Our magic dropdown tag!
        public string AssetID;

        // The Manager will call this method to enforce the state
        public void ApplyState(AMES_State state)
        {
            switch (state)
            {
                case AMES_State.Disabled:
                    gameObject.SetActive(false);
                    break;
                case AMES_State.Enabled:
                    gameObject.SetActive(true);
                    break;
            }
        }

        private void Start()
        {
            // If we spawn at runtime, register with the Manager to sync our state
            if (AMES_Manager.Instance != null)
            {
                AMES_Manager.Instance.RegisterLatecomer(this);
            }
        }
    }
}
