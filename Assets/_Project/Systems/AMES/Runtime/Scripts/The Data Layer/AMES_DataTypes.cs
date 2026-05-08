using UnityEngine;

namespace AMES.Runtime
{
    public class AMES_DataTypes
    {
        // The modular state. We can add Materials, Transforms, etc., later.
        public enum AMES_State
        {
            Disabled = 0,
            Enabled = 1
        }

        // The Instruction payload that tells an asset what to do
        [System.Serializable]
        public struct AMES_Instruction
        {
            [AMES_AssetID] [Tooltip("The ID of the Asset from the Master Database")]
            public string AssetID; 
            [Tooltip("The state this asset should be in")]
            public AMES_State TargetState;
        }
    }
}
