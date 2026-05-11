using System.Collections.Generic;
using UnityEngine;
using static AMES.Runtime.AMES_DataTypes;

namespace AMES.Runtime
{
    // 1. The Snapshot Data Structure
    [System.Serializable]
    public class AMES_Snapshot
    {
        [Tooltip("The Event Index this snapshot was taken at")]
        public int EventIndex;

        [Tooltip("The exact state of ALL assets at this specific moment")]
        public List<AMES_Instruction> FullBoardState = new List<AMES_Instruction>();
    }

    // 2. The Timeline Map
    [CreateAssetMenu(fileName = "AMES_Timeline", menuName = "AMES/3. Timeline Map")]
    public class AMES_TimelineMap : ScriptableObject
    {
        [Tooltip("Drag and drop your Event Nodes here in chronological order")]
        public List<AMES_EventNode> OrderedEvents = new List<AMES_EventNode>(); 
        [HideInInspector] // We hide this because the Editor script will auto-fill it!
        public List<AMES_Snapshot> BakedSnapshots = new List<AMES_Snapshot>();
    }
}
