using Godot;

namespace SA4E.scripts.renderer;

[GlobalClass]
public partial class FireflyGridRenderer2D : FireflyRendererBase2D
{
    public override void _Draw()
    {
        base._Draw();
        
        if (FireflyGrid == null ||
            FireflyGrid.Fireflies == null || FireflyGrid.Fireflies.Length == 0)
            return;
        
        var GridSize = FireflyGrid.GridSize;
		
        //(1) Get the size of the viewport
        var viewportSize = GetViewportRect().Size;
		
        //(2) Calculate the size for each firefly
        var fireflySize = viewportSize / GridSize;
		
        //(3) Draw the fireflies (phase = 0.0 -> black, phase = 1.0 -> white)
        for (int y = 0; y < GridSize.Y; y++)
        {
            for (int x = 0; x < GridSize.X; x++)
            {
                var phase = (float)FireflyGrid.GetPhase(x, y);
                var color = FireflyColor;
                color.A = phase;
                var position = new Vector2(x * fireflySize.X, y * fireflySize.Y);
                var size = fireflySize;
                DrawRect(new Rect2(position, size), color);
            }
        }
    }
}