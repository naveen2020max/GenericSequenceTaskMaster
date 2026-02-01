# GenericSequenceTaskMaster



Technical Design Document: Generic Sequence Task Master



1\. Executive Summary

A modular, node-based sequence engine using a Data-Logic-View separation. It supports complex branching, parallel execution, and recursive sub-graphs, ensuring that level state remains consistent even after scene resets through a Blackboard-driven synchronization pattern.

1.2 Core Architectural Pillars

•	Logic/View Separation: All execution logic resides in pure C# "Processor" classes. MonoBehaviours are restricted to data registration and visual execution (View).

•	Data-Driven: Sequences are authored as ScriptableObject Graphs.

•	Asynchronous Flow: Built on UniTask to handle time-based operations without blocking the main thread.

Stateful Reliability: Uses a Blackboard system to ensure the scene state is consistent across hard resets and checkpoint reloads.

2\. Component Architecture

2.1 The Data Layer (ScriptableObjects)

•	SequenceGraphSO: The asset containing the node network and entry point.

•	BaseNodeData: Abstract base containing a GUID (for saving) and port connections.

•	ActionNodeData: Configuration for a single atomic task.

•	CompositeNodeData: A "Bundle" node containing a list of ActionNodeData to be run as a single unit (Parallel or Sequential).

•	SubGraphNodeData: A node that references another SequenceGraphSO.

2.2 The Logic Layer (Pure C#)

•	ITaskProcessor: Interface for all execution logic.

•	ReflectionFactory: Maps NodeData to Processors using attributes (e.g., \[Processor(typeof(MoveNode))]).

•	SequenceExecutor: Manages the Execution Stack. If a Sub-Graph is triggered, the parent graph is pushed to the stack and the child begins.

2.3 The State Layer (Blackboard)

•	Blackboard: A hierarchical key-value store.

o	Main Blackboard: Global level state.

o	Local Blackboard: Created for Sub-Graphs. It checks itself for a value first; if not found, it "bubbles up" to the Main Blackboard (Inheritance).

•	IStateSync: Interface for MonoBehaviours to snap to the correct state upon registration based on Blackboard data.



3\. System UML Diagram



&nbsp;





4\. How the System Works

Step 1: Authoring (The "Double-Click" Workflow)

I decided on a Graph-Based UI because linear lists fail as soon as you have branches. By allowing Sub-Graphs, we solve the "Messy Graph" problem. If a sequence gets too big, you collapse it into a Sub-Graph node. Double-clicking it simply switches the Editor window's focus to that asset.

Step 2: Discovery (The Reflection Bridge)

When the game starts, how does MoveNodeData know it needs MoveProcessor?

•	Decision: We use Reflection to look for a \[Processor] attribute.

•	Why: This follows the Open-Closed Principle. You can add new gameplay features (like a "Weather Change" task) by just creating a new Data script and a Logic script. You never have to touch the SequenceExecutor or a giant switch statement again.

Step 3: Scene Linkage (The Registry)

Since the Logic is pure C#, it can't see the scene.

•	Decision: A Runtime Registry.

•	Why: This decouples the "Who" from the "What." The task doesn't care if "NPC\_Bob" is a 3D model, a 2D sprite, or a UI icon. As long as it implements IMovable, the task can move it.

Step 4: Handling the "Hard Reset" (The Blackboard Sync)

This is the most critical part of your request.

•	Decision: The Blackboard is the Single Source of Truth.

•	Why: When the scene resets, the Monobehaviours are destroyed and recreated. By having them "Ask" the Blackboard for their state upon rebirth, we ensure the world looks exactly as it should for the current checkpoint.

Step 5: Recursion (The Execution Stack)

How do we handle Sub-Graphs?

•	Decision: The Stack.

•	Why: When a Sub-Graph starts, the Executor "freezes" the current graph state (Node GUID and Blackboard) and pushes it onto a stack. It runs the child. When the child finishes, it pops the parent back off. This allows for infinitely nested sequences.

Step 6: Local Overrides (The Hierarchical Blackboard)

•	Decision: A "Parent" reference in the Blackboard class.

•	Why: If a Sub-Graph needs a variable called Speed, it can have its own local Speed = 10. If it looks for PlayerHealth, it won't find it locally, so it automatically checks the Global Blackboard. This makes your sub-tasks much more powerful and reusable.



5\. Implementation Detail: Core Infrastructure

5.1 Interface: ITaskTarget

Purpose: This is the "Base Tag" for any MonoBehaviour that can be manipulated by a task.

•	Logic: Pure C# logic will only ever see objects as ITaskTarget (or more specific interfaces like IDoor). This prevents the logic from being coupled to specific Unity classes.

5.2 Interface: IStateSync

Purpose: Allows an object to recover its state after a scene reset.

•	Logic: When an object registers itself, it calls Sync(). It looks at the Blackboard and says "The data says I should be closed, so I will snap to closed."

5.3 MonoBehaviour: TaskIdentity

Purpose: The bridge between the Unity Scene and the Registry.

•	Logic: You attach this to a GameObject and give it a UniqueID (e.g., "Player", "MainGate"). It handles automatic registration and unregistration.

5.4 Pure C#: RuntimeRegistry

Purpose: A scene-specific lookup table.

•	Logic: Provides a way for the SequenceExecutor to find an object by its string ID.

5.5 Pure C#: Blackboard

Purpose: The "Single Source of Truth" for game state.

•	Logic: A recursive dictionary that supports local overrides for Sub-Graphs.



6\. Assembly Structure

To keep logic pure and separated from Unity, we will split the project into three distinct assemblies. Add this to your TDD:

A. SequenceSystem.Domain (Pure C#)

•	Dependencies: None (or just UniTask).

•	Contents: ITaskTarget, IStateSync, Blackboard, RuntimeRegistry, ITaskProcessor, and all Data classes (ScriptableObjects).

•	Why: This assembly has no idea that Unity's GameObject or Transform exists. It only knows about interfaces.

B. SequenceSystem.Runtime (Unity-Link)

•	Dependencies: SequenceSystem.Domain.

•	Contents: TaskIdentity, SequenceExecutor (MonoBehaviour version), and Concrete Task implementations that interact with Unity (e.g., UnityMoveTarget).

•	Why: This is the "Glue" layer.

C. SequenceSystem.Editor

•	Dependencies: SequenceSystem.Domain, SequenceSystem.Runtime.

•	Contents: The Graph View editor and custom inspectors.



Summary:

•	Assembly Definitions: Divided into Domain, Runtime, and Editor.

•	Registry Injection: TaskIdentity objects are passive; they are populated by the SequenceBootstrapper during Awake.

•	Decoupling: Scene objects only interact with the Domain through interfaces.



7\. Implementation Detail: The Execution Engine

7.1 The Task Result (Failure Handling)

Instead of a simple void, our tasks return a TaskResult.

•	Enum TaskStatus: Success, Failure, Aborted.

•	Logic: If a processor returns Failure, the SequenceExecutor looks for a "Failure Port" on the graph node to decide where to go next.

7.2 The Execution Context (ExecutionContext)

A data-only class passed into every processor.

•	Contents: References to Blackboard, RuntimeRegistry, and the CancellationToken.

•	Why: This makes processors "pure." They don't look for globals; they only use what’s in the suitcase.

7.3 The Processor Factory (Service)

A class that stays in the SequenceSystem.Runtime assembly.

•	Logic: It uses Reflection once to map all \[Processor] attributes to types and caches them. It provides a CreateProcessor(BaseNodeData data) method.

7.4 The Sequence Executor

The central hub. It manages the Stack<GraphState> to handle sub-graphs and loops.



8\. Task Execution Flow (The "Safety Wrap")

To handle Timeouts and Explicit Failure, the Executor does not just "run" a processor. It "wraps" it:

1\.	It creates a TimeoutController using the duration defined in the NodeData.

2\.	It uses UniTask.WhenAny to race the Processor's Logic against the Timeout Timer.

3\.	If the Timer wins, the Executor forces a cancellation and returns TaskStatus.Timeout.

4\.	If the Processor fails, it returns TaskStatus.Failure.

5\.	The Executor then looks at the Graph to find the next GUID associated with that specific status.



9\. Implementation Detail: The Data Layer 

•	9.1 Anemic Data Model: ScriptableObjects (BaseNodeData, SequenceGraphSO) are treated as "Pure Data" containers. They hold the configuration (GUIDs, Timeouts, Connections) but contain zero execution logic.

•	9.2 Explicit Port Logic: Every node utilizes explicit fields for Success, Failure, and Timeout connections. This allows for clear, branching flow control managed by the SequenceExecutor.

•	9.3 Recursive Composition: The CompositeNodeData allows a single node to act as a container for other nodes. This enables complex "Bundled" actions that can be executed either in sequence or in parallel.

•	9.4 Graph Lookup: The SequenceGraphSO implements a cached dictionary lookup to ensure that jumping between nodes by GUID is an O(1) operation, preventing performance hits in large graphs.

•	9.5 Registry Consistency: All sub-tasks and composites resolve scene objects via the same Global Registry, ensuring a shared environment context across all levels of recursion.



&nbsp;





10\. Implementation Detail: State Branching \& Scoping (Step 5)

•	10.1 Comparison Engine: Introduced ComparisonNodeData, allowing the sequence to branch based on Blackboard values. The ComparisonProcessor handles standard mathematical operators.

•	10.2 Hierarchical Scoping: When a Sub-Graph is executed, the system automatically instantiates a "Child Blackboard." This child can read all data from the Global/Parent Blackboard but writes all new data to its own local scope, preventing accidental state corruption in the parent graph.

•	10.3 String-Based Addressing: Utilizes string keys for Blackboard lookups to maintain maximum flexibility across different projects, with the architectural intent to implement "Key Discovery" at the Editor level later.

Architectural Review: The Core is Complete

The System can :

•	Find scene objects (Registry).

•	Store global/local state (Blackboard).

•	Run tasks asynchronously (UniTask).

•	Run parallel/sequential logic with fail-fast (Composites).

•	Branch based on state (Comparison).

•	Nest sequences infinitely (Sub-Graphs).



12\. Implementation Detail: Lifecycle \& Persistence 

•	12.1 Persistence DTO: Blackboard state is converted to a BlackboardEntry\[] for serialization. This allows complex state to be saved into standard JSON/Binary save files.

•	12.2 Async Resolution: The Registry now supports a "Wait-and-Retry" logic for resolving scene targets, mitigating race conditions during scene initialization or heavy loading frames.

•	12.3 Explicit Cancellation: The SequenceRunner manages the CancellationTokenSource lifecycle, ensuring that all tasks are killed if the GameObject is destroyed or the sequence is restarted.

•	12.4 Diagnostic Logging: (Optional) The Executor now includes hooks for tracking node execution, which will later be used by the GraphView to "highlight" the active node.

13\. Final Backend Integrity 

•	13.1 Strict State Validation: The Blackboard now utilizes TryGet logic. If a processor attempts to access a non-existent variable, the task is automatically marked as Failed, preventing silent logic errors during gameplay.



•	13.2 Event-Driven Failure: The SequenceExecutor exposes an OnSequenceFailed event. This allows external systems (e.g., a Level Manager) to handle "Hard Resets" or "UI Failure Screens" without the Sequence system needing a direct reference to the Scene Manager.



•	13.3 Sequential Safety: Added logging hooks to identify exactly which GUID failed, ensuring that even in complex nested sub-graphs, the developer knows exactly where the logic chain broke.











