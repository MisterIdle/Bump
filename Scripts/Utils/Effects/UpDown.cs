using Godot;
using System;

namespace Com.IsartDigital.Shmup.NToolsBox;

public partial class UpDown : Sprite2D
{
    [Export] public float amplitudeMin = 3f;
    [Export] public float amplitudeMax = 10f;

    [Export] public float speedMin = 0.3f;
    [Export] public float speedMax = 2f;

    private Vector2 startPos;
    private float amplitude;
    private float speed;
    private float time;

    public override void _Ready()
    {
        startPos = Position;

        var rng = new RandomNumberGenerator();
        rng.Randomize();

        amplitude = rng.RandfRange(amplitudeMin, amplitudeMax);
        speed = rng.RandfRange(speedMin, speedMax);
        time = rng.RandfRange(0f, Mathf.Pi * 2f);
    }

    public override void _Process(double delta)
    {
        time += (float)delta;

        Position = startPos + new Vector2(
            0,
            Mathf.Sin(time * speed) * amplitude
        );
    }
}
