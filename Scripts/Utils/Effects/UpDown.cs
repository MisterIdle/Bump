using Godot;
using System;

namespace Com.IsartDigital.Shmup.NToolsBox;

public partial class UpDown : Sprite2D
{
    [Export] public float amplitude = 1f;
    [Export] public float speed = 2;

    private Vector2 startPos;
    private float time = 0;

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

        float lYOffset = Mathf.Sin(time * speed) * amplitude;

        Position = startPos + new Vector2(0, lYOffset);
    }
}