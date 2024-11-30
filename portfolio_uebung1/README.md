# Fireflies Behaviour Simulation Project

## Overview
This project is a simulation of fireflies' behavior, supporting both singleplayer and multiplayer modes. The simulation involves initializing a grid of fireflies and synchronizing their phases to simulate group behavior. In multiplayer mode, both server and clients are connected as fireflies, and their phases are synchronized across the network.

## Table of Contents
1. Structure
2. Classes, Attributes, and Methods
3. Execution Order
4. Additional Information

## 1. Structure
The project is structured to support both singleplayer and multiplayer modes. The main entry point is the `Application` class, which initializes and configures the simulation based on the mode selected.

## 2. Classes, Attributes, and Methods

### Application
- **Attributes:**
    - `Instance`: Singleton instance of the Application.
    - `IsOnlineEnabled`: Indicates whether the online mode is enabled.
    - `DefaultGridWidth`: Default grid width for the application.
    - `DefaultGridHeight`: Default grid height for the application.
    - `DefaultCoupling`: Default coupling value for the application.
    - `SingleplayerApp`: Instance of the SingleplayerApp.
    - `MultiplayerApp`: Instance of the MultiplayerApp.
- **Methods:**
    - `Main()`: Main method to initialize the application.
    - `_Ready()`: Called when the node enters the scene tree for the first time.
    - `_Notification(int what)`: Handles notifications for the application.

### SingleplayerApp
- **Attributes:**
    - `FireflyGrid`: Instance of the FireflyStructGridNode2D.
    - `FireflyGridRenderer`: Instance of the FireflyGridRenderer2D.
- **Methods:**
    - `Main(int gridWidth, int gridHeight, bool isTorus, double coupling)`: Initializes the fireflies and starts their tasks.
    - `Stop()`: Stops the rendering task.

### MultiplayerApp
- **Attributes:**
    - `IsServer`: Indicates if the instance is a server.
    - `ServerAddress`: Address of the server.
    - `ServerPort`: Port of the server.
    - `MaximumClients`: Maximum number of clients.
    - `IsRunning`: Indicates if the multiplayer is running.
    - `FireflyGrid`: Instance of the FireflyStructGridNode2D.
    - `PeerFireflyID`: ID of the firefly assigned to this peer.
    - `PeerFireflyIndex`: Index of the firefly assigned to this peer.
    - `UpdateCancellationTokenSource`: Cancellation token source for update tasks.
    - `UpdatePhaseTask`: Task for updating the phase.
    - `FireflyRenderer`: Instance of the FireflyInstanceRenderer2D.
- **Methods:**
    - `RequestAssignPeer(long peerId, out int fireflyId, out Vector2I fireflyIndex)`: Requests assignment of a peer to a firefly.
    - `RequestReleasePeer(long peerId)`: Requests release of a peer from a firefly.
    - `ResponseAssignPeer(long targetPeerId, int fireflyId, Vector2I fireflyIndex)`: Responds to the assignment of a peer to a firefly.
    - `SetPhaseRpc(long senderPeerId, Vector2I index, double phase)`: Sets the phase of a firefly via RPC.
    - `SetPhase()`: Calculates and sets the phase of the assigned firefly.
    - `UpdatePhaseAsync()`: Asynchronous task to update the phase of the assigned firefly.
    - `Start()`: Starts the multiplayer mode.
    - `Stop()`: Stops the multiplayer mode.
    - `Main(int gridWidth, int gridHeight, bool isTorus, double coupling, bool isServer)`: Initializes the multiplayer mode.

### FireflyStructGridNode2D
- **Attributes:**
    - `Instance`: Singleton instance of the FireflyStructGridNode2D.
    - `GridSize`: Size of the grid.
    - `IsTorus`: Indicates if the grid is a torus.
    - `Coupling`: Coupling value for the fireflies.
    - `Fireflies`: Array of firefly structures.
    - `UpdateCancellationTokenSource`: Cancellation token source for update tasks.
    - `FireflyUpdateTasks`: Array of tasks for updating fireflies.
- **Methods:**
    - `Initialize(int width, int height, bool isTorus, double coupling)`: Initializes the grid of fireflies.
    - `GetPhase(int x, int y)`: Gets the phase of a firefly at the specified position.
    - `SetPhase(int x, int y, double phase)`: Sets the phase of a firefly at the specified position.
    - `TryAssignPeer(long peerId, out int id, out Vector2I index)`: Tries to assign a peer to a firefly.
    - `TryReleasePeer(long peerId)`: Tries to release a peer from a firefly.
    - `GetPhaseSum4Neighbours(Vector2I pos, out double phaseSum, out int neighbourCount)`: Gets the sum of the phases of the four neighbors.
    - `StartUpdatePhaseTasks()`: Starts the update tasks for the fireflies.
    - `StopTasks()`: Stops the update tasks.

### FireflyNeighbor



### FireflyStruct
- **Attributes:**
    - `ID`: Identifier of the firefly.
    - `Index`: Index of the firefly in the grid.
    - `Base`: Base value for phase calculation.
    - `Coupling`: Coupling value for phase calculation.
    - `Phase`: Current phase of the firefly.
    - `AssignedPeerID`: ID of the assigned peer.
- **Methods:**
    - `CalculatePhase()`: Calculates the new phase of the firefly.
    - `UpdatePhaseTask()`: Task to update the phase of the firefly.
    - `TryAssignPeer(long peerId, out int id, out Vector2I index)`: Tries to assign a peer to the firefly.
    - `TryReleasePeer(long peerId)`: Tries to release a peer from the firefly.

### FireflyRendererBase2D
- **Attributes:**
    - `FireflyColor`: Color of the fireflies.
    - `FireflyGrid`: Instance of the FireflyStructGridNode2D.
    - `RenderingTask`: Task for rendering.
    - `RenderingCancellationTokenSource`: Cancellation token source for rendering.
- **Methods:**
    - `StartRenderingTask()`: Starts the rendering task.
    - `StopRenderingTask()`: Stops the rendering task.

### FireflyInstanceRenderer2D (inherits FireflyRendererBase2D)
- **Attributes:**
    - `FireflyID`: ID of the firefly to render.
    - `FireflyIndex`: Index of the firefly to render.
- **Methods:**
    - `_Draw()`: Draws the firefly.

### FireflyGridRenderer2D (inherits FireflyRendererBase2D)
- **Methods:**
    - `_Draw()`: Draws the grid of fireflies.




## 3. Execution Order
1. `Application._Ready()`: Entry point of the application.
2. `Application.Main()`: Initializes the application.
3. `SingleplayerApp.Main()` or `MultiplayerApp.Main()`: Initializes the fireflies and starts the respective mode.
4. `FireflyStructGridNode2D.Initialize()`: Initializes the grid of fireflies.
5. `SingleplayerApp.StartUpdatePhaseTasks()` or `MultiplayerApp.Start()`: Starts the update tasks for the fireflies.
6. `FireflyRendererBase2D.StartRenderingTask()`: Starts the rendering task.

## 4. Additional Information
For more detailed information about the specific tasks and projects solved, please refer to the following documents:
- `readme_aufgabe1.md`
- `readme_aufgabe2.md`