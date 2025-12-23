using Godot;
using System;

public partial class CameraManager : Camera2D
{
    public override void _Process(double pDelta)
    {
        float lDelta = (float)pDelta;
        Position += GameManager.scrollSpeedVelocity * lDelta;
    }
}
