Here is the formal Architectural Design Document for the Asset Management Enforcer System (AMES). 



# **System Design Document: Asset Management Enforcer System (AMES)**



###### **Role:** Game Architecture Blueprint 

###### **Target Engine:** Unity 3D 

###### **System Type:** Deterministic State Manager \& Event Enforcer



#### **1. Executive Summary**



The Asset Management Enforcer System (AMES) is a centralized, highly optimized architecture designed to manage and enforce the state of static level assets (e.g., doors, bridges, environmental triggers). By decoupling asset references from game logic and treating state changes as a deterministic timeline, AMES guarantees bulletproof game progression, immediate resolution of missed events, and instantaneous scene initialization.



#### **2. Core Objectives**



* Decoupled State Management (Goal 1): Act as the sole router for changing static asset states in the scene, eliminating spaghetti code and "God Object" dependencies.
* Deterministic Enforcement (Goal 2): Maintain a chronological Map of events. If sequence breaking occurs, calculate and enforce missed states mathematically without manual triggers.
* Performance First: Guarantee O(1) state resolution at runtime, zero frame-drops during scene initialization, and minimal RAM footprint.



#### **3. High-Level Architecture (The 3 Layers)**



###### AMES is divided into three strict layers to separate data, logic, and execution:



###### **Layer 1: The Data Layer (ScriptableObjects)**



* Responsibility: Define "What happens" and "When".
* Structure: Authored completely in the Unity Editor by designers. Contains no runtime logic.
* Key Components: Master Database, Event Nodes, Timeline Map.



###### **Layer 2: The Core Enforcer (AMES Manager)**



* Responsibility: Act as the "Brain" for the current scene.
* Structure: A lightweight Scene-Local manager. Reads Layer 1 and sends commands to Layer 3.
* Key Components: The Blackboard (Current State Cache), Event Fast-Forward Calculator.



###### **Layer 3: The Assets (AMES Agents)**



* Responsibility: Execute the commands locally on the GameObjects.
* Structure: "Dumb" components attached to 3D models, UI, etc.
* Key Components: AMES\_Agent script (Listens to Layer 2 and applies SetActive).



#### **4. Component \& Data Breakdown**



###### **A. The Master Database (Fixing ID Bloat)**



* A single global ScriptableObject (AMES\_MasterDatabase).
* Contains an Editor-managed list of strings (Asset IDs) categorized by type (e.g., "Bridge\_Level1", "Boss\_Door").
* Benefit: Prevents the creation of 10,000 tiny ID files. Acts as the single source of truth for all valid AMES targets in the game.



B. The Timeline Map \& Event Nodes



&#x20; - Event Node: A ScriptableObject defining an action (e.g., Event Name: "Bridge

&#x20;   Explodes", Instruction: \[Bridge\_Level1 : Disabled]).

&#x20; - Timeline Map: An ordered list of Event Nodes representing the linear

&#x20;   sequence of the level.

&#x20; - Note on Scope: Events only record Static States (bridge disabled), never

&#x20;   Transient Actions (explosions, particle effects).



C. The AMES Manager \& The Blackboard



&#x20; - The Blackboard: A runtime Dictionary tracking the current enforced state of

&#x20;   every registered asset ID.

&#x20; - When AMES executes an event, it updates the Blackboard first, then pushes

&#x20;   the command to the Agents.

&#x20; - Benefit: Runtime-spawned assets (latecomers) query the Blackboard instantly

&#x20;   without triggering a timeline recalculation.



D. The AMES Agent



&#x20; - A component attached to GameObjects.

&#x20; - Features a Custom Inspector UI: Designers cannot type IDs; they must select

&#x20;   from a Dropdown populated by the Master Database.

&#x20; - Receives payloads from the Manager and executes them (currently focused on

&#x20;   gameObject.SetActive()).



5\. Optimization \& Performance Solutions



To make AMES AAA-ready, the system relies heavily on Editor-Time Pre-Baking.



Optimization 1: "Pre-Baked" Scene Registration



&#x20; - Problem Solved: Initialization lag spikes and the "Graveyard Paradox"

&#x20;   (disabled objects cannot run Start()).

&#x20; - Implementation: A custom Editor button (Bake Scene Assets) scans the scene

&#x20;   for all AMES\_Agent components and permanently saves their references into

&#x20;   the Manager's array.

&#x20; - Result: Zero runtime registration cost. The Manager can turn on completely

&#x20;   disabled GameObjects because it holds their memory address directly from the

&#x20;   scene file.



Optimization 2: The Keyframe + Delta Approach



&#x20; - Problem Solved: High CPU/Memory usage when fast-forwarding/skipping massive

&#x20;   amounts of events.

&#x20; - Implementation:

&#x20;     - Deltas: Normal Event Nodes only store what changed (e.g., 2 objects

&#x20;       changed).

&#x20;     - Keyframes: An Editor tool (Bake Timeline) simulates the level and saves

&#x20;       a full State Snapshot of all objects every 10 Events.

&#x20; - Result: If a player skips to Event 47, AMES loads Keyframe 40 (O(1) cost),

&#x20;   and calculates the Deltas for 41-47. Memory usage drops by 90%, and CPU

&#x20;   calculations are strictly capped at a maximum of 9 small steps.



6\. System Integrations



Interaction with the Save System



&#x20; - AMES does not touch the disk.

&#x20; - The global Save/Load System queries AMES: "What is the Current Event Index?"

&#x20;   and saves that single Integer to the player's file.

&#x20; - On load, the Save System passes the Integer to the Scene-Local AMES, which

&#x20;   loads the closest Keyframe and fast-forwards.



Interaction with Scene Management



&#x20; - AMES is Scene-Local. It does not use DontDestroyOnLoad.

&#x20; - Every level has its own autonomous AMES Manager and its own Timeline Map.

&#x20; - This strictly prevents cross-scene memory leaks and null-reference

&#x20;   exceptions when scenes are unloaded.



7\. Accepted Constraints \& Mitigations



1\.  Constraint: Workflow Friction. Developers must remember to click "Bake

&#x20;   Scene" after adding new assets, otherwise, they will not be registered.

&#x20;     - Mitigation: Implement a pre-build hook that automatically runs the Bake

&#x20;       script before compiling the game.

2\.  Constraint: Procedural Incompatibility. AMES relies on Editor pre-baking,

&#x20;   making it fundamentally incompatible with randomly generated dungeons.

&#x20;     - Mitigation: Limit AMES to hand-crafted, deterministic game spaces.

3\.  Constraint: Instant Teleportation. Fast-forwarding applies state instantly,

&#x20;   which bypasses Unity Animator transitions.

&#x20;     - Mitigation: Define strict rules where AMES sets "Final State" overrides,

&#x20;       and Agent scripts handle the logic to either snap instantly (if loading)

&#x20;       or play an animation (if playing live).



8\. Future Expandability



While the Minimum Viable Product (MVP) focuses on SetActive, the Payload

structure between Layer 2 and Layer 3 is modular. Future iterations can easily

support:



&#x20; - Component Toggling: Disabling MeshRenderer or Collider while keeping the

&#x20;   object active.

&#x20; - Transform Swapping: Snapping objects to predefined Vector3 positions.

&#x20; - Material Overrides: Swapping a "Pristine" material for a "Destroyed"

&#x20;   material based on the State Blackboard.



End of Document



