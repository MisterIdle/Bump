using Godot;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NManager;

public partial class Camera : Camera2D
{
    private static Camera instance;

    public static Camera GetInstance() => instance;

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else
        {
            QueueFree();
            return;
        }
    }

    public override void _Process(double pDelta)
    {
        float lDelta = (float)pDelta;

        Vector2 lMove = GameManager.scrollSpeedVelocity * ScaleTime.GetDelta(lDelta);
        Position += lMove;
    }

    protected override void Dispose(bool pDisposing)
    {
        if (instance == this) instance = null;
        base.Dispose(pDisposing);
    }
}