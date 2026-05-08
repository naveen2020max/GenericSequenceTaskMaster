using System.Collections.Generic;
using UnityEngine;
using static AMES.Runtime.AMES_DataTypes;

namespace AMES.Runtime
{
    [CreateAssetMenu(fileName = "New_AMES_Event", menuName = "AMES/2. Event Node")]
    public class AMES_EventNode: ScriptableObject
    {
        [Tooltip("A human-readable name for this event (e.g., 'Boss Defeated')")]
        public string EventName;

        [Tooltip("The list of assets that change state during this exact event")]
        public List<AMES_Instruction> Instructions = new List<AMES_Instruction>();
    }
}
