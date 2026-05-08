using System.Collections.Generic;
using UnityEngine;

namespace AMES.Runtime
{
    [CreateAssetMenu(fileName = "AMES_MasterDatabase", menuName = "AMES/1. Master Database")]
    public class AMES_MasterDatabase : ScriptableObject
    {
        [Tooltip("Add all your unique Asset IDs here (e.g., Bridge_Level1, Castle_Door)")]
        public List<string> RegisteredAssetIDs = new List<string>();

        // We will use this later for our Custom Editor Dropdown!
        public string[] GetAssetIDArray()
        {
            return RegisteredAssetIDs.ToArray();
        }
    }
}
