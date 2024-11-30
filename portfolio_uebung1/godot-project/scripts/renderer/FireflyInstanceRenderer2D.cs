using System.Threading;
using System.Threading.Tasks;
using Godot;
using SA4E.scripts.fireflies;

namespace SA4E.scripts.renderer;

[GlobalClass]
public partial class FireflyInstanceRenderer2D : FireflyRendererBase2D
{
    [Export] public int FireflyID;
    [Export] public Vector2I FireflyIndex;
    
    public override void _Draw()
    {
        base._Draw();
        
        if (FireflyGrid == null ||
            FireflyGrid.Fireflies == null || FireflyGrid.Fireflies.Length == 0)
        {
            GD.Print("FireflyInstanceRenderer2D._Draw - FireflyGrid is null or empty!");
            return;
        }
        
        //(1) Calculate the size for each firefly
        var position = Vector2I.Zero;
        var fireflySize = GetViewportRect().Size;
        
        //(3) Draw the single firefly instance (phase = 0.0 -> black, phase = 1.0 -> white)
        var phase = (float)FireflyGrid.GetPhase(FireflyIndex);
        var color = FireflyColor;
        color.A = phase;
        
        //(4) Draw the firefly
        DrawRect(new Rect2(position, fireflySize), color);
    }
}