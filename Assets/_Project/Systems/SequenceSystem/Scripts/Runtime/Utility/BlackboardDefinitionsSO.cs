using System.Collections.Generic;
using UnityEngine;

namespace SequenceSystem.Runtime
{
    // This asset acts as the "Dictionary" of valid variable names for your project.
    [CreateAssetMenu(menuName = "Sequence/Config/Blackboard Definitions")]
    public class BlackboardDefinitionsSO : DefinitionsSO
    {
        
    }

    public class DefinitionsSO : ScriptableObject
    {
        [Tooltip("List of all valid keys used in the Blackboard")]
        public List<string> DefinedKeys = new List<string>();
    }
}