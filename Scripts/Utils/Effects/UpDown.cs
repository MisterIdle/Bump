using Godot;
using System;

namespace Com.IsartDigital.Shmup.NToolsBox;

public partial class UpDown : Sprite2D
{
    [Export] public float amplitude = 50f;
    [Export] public float speed = 2f;

    private Vector2 startPos;
    private float time = 0f;


    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        startPos = Position;
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    public override void _Process(double pDelta)
    {
        time += (float)pDelta;
        Position = startPos + new Vector2(0, Mathf.Sin(time * speed) * amplitude);
    }
}
