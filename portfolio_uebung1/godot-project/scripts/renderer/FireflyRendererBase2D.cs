using System.Threading;
using System.Threading.Tasks;
using Godot;
using SA4E.scripts.fireflies;

namespace SA4E.scripts.renderer;

[GlobalClass]
public partial class FireflyRendererBase2D : Node2D
{
    [Export] public Color FireflyColor { get; set; } = new Color(1, 1, 1);
    
    [Export] public FireflyStructGridNode2D FireflyGrid { get; private set; }
    
    
    public Task RenderingTask { get; protected set; }
    public CancellationTokenSource RenderingCancellationTokenSource { get; protected set; }

    private bool IsTaskRunning()
    {
        return RenderingTask is { Status: TaskStatus.Running };
    }

    private async Task ReRenderAsync()
    {
        GD.Print("FireflyRendererBase2D.RequestRender - Started!");
        
        while (!RenderingCancellationTokenSource.Token.IsCancellationRequested)
        {
            CallDeferred("queue_redraw");
            await Task.Delay(5); //1000/5 = 200 FPS
        }
        
        GD.Print("FireflyRendererBase2D.RequestRender - Stopped!");
    }
    public void StartRenderingTask()
    {
        if (IsTaskRunning())
            return;
        
        RenderingCancellationTokenSource = new CancellationTokenSource();
        RenderingTask = Task.Run(ReRenderAsync);
    }
    public void StopRenderingTask()
    {
        if (!IsTaskRunning())
            return;
        
        RenderingCancellationTokenSource.Cancel();
    }
}