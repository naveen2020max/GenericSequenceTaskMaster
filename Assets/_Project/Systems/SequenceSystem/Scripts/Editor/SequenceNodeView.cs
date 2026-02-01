// File: SequenceNodeView.cs (Editor Assembly)
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using SequenceSystem.Runtime;

namespace SequenceSystem.Editor
{
    public class SequenceNodeView : Node
    {
        public BaseNodeData NodeData { get; private set; }
        public Port InputPort { get; private set; }
        public Port SuccessPort { get; private set; }
        public Port FailurePort { get; private set; }
        public Port TimeoutPort { get; private set; }

        public SequenceNodeView(BaseNodeData nodeData)
        {
            NodeData = nodeData;
            title = nodeData.name; // Use the asset name
            viewDataKey = nodeData.Guid; // Important for persistence

            style.left = 0;
            style.top = 0;

            CreateInputPorts();
            CreateOutputPorts();
        }

        // Hook into the selection event to update the Unity Inspector
        public override void OnSelected()
        {
            base.OnSelected();
            Selection.activeObject = NodeData;
        }

        private void CreateInputPorts()
        {
            // Standard "Entry" port
            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);
        }

        private void CreateOutputPorts()
        {
            // 1. Success Port (Green)
            SuccessPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            SuccessPort.portName = "Success";
            SuccessPort.portColor = Color.green;
            outputContainer.Add(SuccessPort);

            // 2. Failure Port (Red)
            FailurePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            FailurePort.portName = "Failure";
            FailurePort.portColor = Color.red;
            outputContainer.Add(FailurePort);

            // 3. Timeout Port (Yellow) - Only if enabled
            if (NodeData.UseTimeout)
            {
                TimeoutPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                TimeoutPort.portName = "Timeout";
                TimeoutPort.portColor = Color.yellow;
                outputContainer.Add(TimeoutPort);
            }
        }
    }
}