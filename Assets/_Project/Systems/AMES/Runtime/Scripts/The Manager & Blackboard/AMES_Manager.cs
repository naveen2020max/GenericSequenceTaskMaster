using System.Collections.Generic;
using UnityEngine;
using static AMES.Runtime.AMES_DataTypes;

namespace AMES.Runtime
{
    public class AMES_Manager : MonoBehaviour
    {
        // Scene-Local Singleton so Agents can find it easily
        public static AMES_Manager Instance { get; private set; }
        [Tooltip("The list of agents baked into the scene.")]
        public List<AMES_Agent> PreBakedAgents = new List<AMES_Agent>();

        // THE BLACKBOARD: Tracks the current deterministic state of any ID
        private Dictionary<string, AMES_State> Blackboard = new Dictionary<string, AMES_State>();

        // QUICK LOOKUP: Groups Agents by their ID so we don't have to search the scene
        private Dictionary<string, List<AMES_Agent>> ActiveAgentsMap = new Dictionary<string, List<AMES_Agent>>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Initialize our Quick Lookup map using the pre-baked agents
            foreach (var agent in PreBakedAgents)
            {
                if (agent != null) InternalRegister(agent);
            }
        }

        // --- CORE LOGIC 1: EXECUTING A SINGLE EVENT ---
        public void ExecuteEvent(AMES_EventNode eventNode)
        {
            if (eventNode == null) return;

            foreach (var instruction in eventNode.Instructions)
            {
                // 1. Update the Blackboard (The single source of truth)
                Blackboard[instruction.AssetID] = instruction.TargetState;

                // 2. Enforce the state on all agents sharing this ID
                if (ActiveAgentsMap.TryGetValue(instruction.AssetID, out List<AMES_Agent> agents))
                {
                    foreach (var agent in agents)
                    {
                        agent.ApplyState(instruction.TargetState);
                    }
                }
            }
        }

        // --- CORE LOGIC 2: FAST-FORWARDING MULTIPLE EVENTS ---
        public void FastForwardEvents(List<AMES_EventNode> eventsToSimulate)
        {
            // 1. Calculate the net result of all missed events into the Blackboard
            foreach (var eventNode in eventsToSimulate)
            {
                foreach (var instruction in eventNode.Instructions)
                {
                    Blackboard[instruction.AssetID] = instruction.TargetState;
                }
            }

            // 2. Enforce the final Blackboard states onto the Scene
            foreach (var kvp in Blackboard)
            {
                string id = kvp.Key;
                AMES_State finalState = kvp.Value;

                if (ActiveAgentsMap.TryGetValue(id, out List<AMES_Agent> agents))
                {
                    foreach (var agent in agents)
                    {
                        agent.ApplyState(finalState);
                    }
                }
            }
        }

        // --- CORE LOGIC 3: LATECOMERS (Objects spawned mid-game) ---
        public void RegisterLatecomer(AMES_Agent newAgent)
        {
            InternalRegister(newAgent);

            // Instantly check the Blackboard. If a state exists for this guy, apply it!
            if (Blackboard.TryGetValue(newAgent.AssetID, out AMES_State requiredState))
            {
                newAgent.ApplyState(requiredState);
            }
        }

        private void InternalRegister(AMES_Agent agent)
        {
            if (!ActiveAgentsMap.ContainsKey(agent.AssetID))
            {
                ActiveAgentsMap[agent.AssetID] = new List<AMES_Agent>();
            }

            // Prevent duplicate registrations
            if (!ActiveAgentsMap[agent.AssetID].Contains(agent))
            {
                ActiveAgentsMap[agent.AssetID].Add(agent);
            }
        }
    }
}
