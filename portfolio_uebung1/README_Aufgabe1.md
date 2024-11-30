# Project Documentation

## Table of Contents
1. Structure
2. Classes, Attributes, and Methods
3. Execution Order

## 1. Structure
This project is a simulation application that supports both singleplayer and multiplayer modes. The singleplayer mode initializes a grid of fireflies and starts their update and rendering tasks.

## 2. Classes, Attributes, and Methods

### Application
- **Attributes:**
    - `Instance`: Singleton instance of the Application.
    - `IsOnlineEnabled`: Indicates whether the online mode is enabled.
    - `DefaultGridWidth`: Default grid width for the application.
    - `DefaultGridHeight`: Default grid height for the application.
    - `DefaultCoupling`: Default coupling value for the application.
    - `SingleplayerApp`: Instance of the SingleplayerApp.
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
    - `StartUpdatePhaseTasks()`: Starts the update tasks for the fireflies.
    - `StopTasks()`: Stops the update tasks.

### FireflyStruct
- **Attributes:**
    - `ID`: Identifier of the firefly.
    - `Index`: Index of the firefly in the grid.
    - `Base`: Base value for phase calculation.
    - `Coupling`: Coupling value for phase calculation.
    - `Phase`: Current phase of the firefly.
    - `AssignedPeerID`: ID of the assigned peer (for multiplayer).
- **Methods:**
    - `CalculatePhase()`: Calculates the new phase of the firefly.
    - `UpdatePhaseTask()`: Task to update the phase of the firefly.

### FireflyGridRenderer2D
- **Attributes:**
    - `FireflyColor`: Color of the fireflies.
    - `FireflyGrid`: Instance of the FireflyStructGridNode2D.
    - `RenderingTask`: Task for rendering.
    - `RenderingCancellationTokenSource`: Cancellation token source for rendering.
- **Methods:**
    - `StartRenderingTask()`: Starts the rendering task.
    - `StopRenderingTask()`: Stops the rendering task.

## 3. Execution Order
1. `Application._Ready()`: Entry point of the application.
2. `Application.Main()`: Initializes the application.
3. `SingleplayerApp.Main()`: Initializes the fireflies and starts their tasks.
4. `FireflyStructGridNode2D.Initialize()`: Initializes the grid of fireflies.
5. `FireflyStructGridNode2D.StartUpdatePhaseTasks()`: Starts the update tasks for the fireflies.
6. `FireflyGridRenderer2D.StartRenderingTask()`: Starts the rendering task.