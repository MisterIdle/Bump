using Godot;
using System;

public partial class CameraManager : Camera2D
{
    private static CameraManager instance;

    public static CameraManager GetInstance()
    {
        return instance;
    }

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
        Position += GameManager.scrollSpeedVelocity * lDelta;
    }
}
