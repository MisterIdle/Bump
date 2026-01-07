using Godot;

using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NManager;

public partial class Camera : Camera2D
{
    private static Camera instance;

    public static Camera GetInstance()
    {
        return instance;
    }

    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else
        {
            QueueFree();
            return;
        }
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    public override void _Process(double pDelta)
    {
        float lDelta = (float)pDelta;

        Position += GameManager.scrollSpeedVelocity * ScaleTime.GetDelta(lDelta);
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}