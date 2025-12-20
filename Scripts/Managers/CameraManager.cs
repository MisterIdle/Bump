using Godot;
using System;

public partial class CameraManager : Camera2D
{
    public override void _Process(double pDelta)
    {
        float lDelta = (float)pDelta;

        // 0 Player pos pour plus tard
        Position += GameManager.scrollSpeedVelocity * lDelta;
    }
}
