using AMES.Runtime;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static AMES.Runtime.AMES_DataTypes;

namespace AMES.Editor
{
    [CustomEditor(typeof(AMES_TimelineMap))]
    public class AMES_TimelineEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            AMES_TimelineMap timeline = (AMES_TimelineMap)target;

            GUILayout.Space(15);
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("BAKE SNAPSHOTS (Every 10 Events)", GUILayout.Height(40)))
            {
                BakeTimelineSnapshots(timeline);
            }
            GUI.backgroundColor = Color.white;
        }

        private void BakeTimelineSnapshots(AMES_TimelineMap timeline)
        {
            timeline.BakedSnapshots.Clear();

            // This temporary dictionary acts as our "Simulation Blackboard"
            Dictionary<string, AMES_State> simBoard = new Dictionary<string, AMES_State>();

            // Loop through every event in the timeline
            for (int i = 0; i < timeline.OrderedEvents.Count; i++)
            {
                AMES_EventNode node = timeline.OrderedEvents[i];
                if (node == null) continue;

                // Apply the event's deltas to our simulation board
                foreach (var instruction in node.Instructions)
                {
                    simBoard[instruction.AssetID] = instruction.TargetState;
                }

                // SNAPSHOT TRIGGER: Every 10th event (Index 9, 19, 29...), OR the very last event
                if ((i + 1) % 10 == 0 || i == timeline.OrderedEvents.Count - 1)
                {
                    AMES_Snapshot newSnapshot = new AMES_Snapshot { EventIndex = i };

                    // Convert the simulation board into a list of instructions
                    foreach (var kvp in simBoard)
                    {
                        newSnapshot.FullBoardState.Add(new AMES_Instruction
                        {
                            AssetID = kvp.Key,
                            TargetState = kvp.Value
                        });
                    }

                    timeline.BakedSnapshots.Add(newSnapshot);
                }
            }

            EditorUtility.SetDirty(timeline);
            Debug.Log($"[AMES] Timeline Baked! Created {timeline.BakedSnapshots.Count} Snapshots.");
        }
    }
}
