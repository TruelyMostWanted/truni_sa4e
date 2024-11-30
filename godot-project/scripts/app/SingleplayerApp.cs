using Godot;
using SA4E.scripts.fireflies;
using SA4E.scripts.renderer;

namespace SA4E.scripts.app;

[GlobalClass]
public partial class SingleplayerApp : Node
{
    [Export] public FireflyStructGridNode2D FireflyGrid { get; private set; }
    [Export] public FireflyGridRenderer2D FireflyGridRenderer { get; private set; }

    public void Main(int gridWidth, int gridHeight, bool isTorus = true, double coupling = 0.1)
    {
        //(1) Initialize the fireflies
        FireflyGrid.Initialize(gridWidth, gridHeight, isTorus, coupling);
        
        //(2) Start their tasks
        FireflyGrid.StartUpdatePhaseTasks();
		
        //(3) Start the rendering task
        FireflyGridRenderer.StartRenderingTask();
    }

    public void Stop()
    {
        //(1) Stop the rendering task
        FireflyGrid.StopTasks();
    }
}