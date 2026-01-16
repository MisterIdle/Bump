using Com.IsartDigital.Shmup.NManager;
using Godot;
using System;

public partial class Parallax : ParallaxBackground
{
    private static Parallax instance;
    public static Parallax GetInstance() => instance;

    [Export] public Panel panel;

    [Export] public float SpawnY = 1500f;
    [Export] public float Duration = 1.5f;
    [Export] public float Stagger = 0.2f;

    [Export] public Vector2 scrollDirection = Vector2.Down;
    [Export] public float normalSpeed = 300f;

    [Export] public Color normalColor = Colors.White;
    [Export] public Color bossColor = new Color(1f, 0.2f, 0.2f);

    private float currentSpeed;

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else
        {
            QueueFree();
            return;
        }

        int i = 0;
        foreach (Node child in GetChildren())
        {
            if (child is ParallaxLayer layer)
            {
                Vector2 finalPos = layer.Position;

                layer.Position = new Vector2(finalPos.Y, SpawnY);

                Tween tween = CreateTween();

                tween.TweenProperty(layer, "position", finalPos, Duration)
                     .SetTrans(Tween.TransitionType.Back)
                     .SetEase(Tween.EaseType.Out)
                     .SetDelay(i * Stagger);
                i++;
            }
        }
    }


    public override void _Process(double delta)
    {
        ScrollBaseOffset += scrollDirection * currentSpeed * (float)delta;
    }

    public void SetBossMode(bool active)
    {
        Color targetColor = active ? bossColor : normalColor;

        Tween tween = CreateTween();
        tween.SetParallel(true);

        if (panel != null)
        {
            tween.TweenProperty(panel, "self_modulate", targetColor, 1.0f)
                 .SetTrans(Tween.TransitionType.Sine)
                 .SetEase(Tween.EaseType.Out);
        }
    }


    public override void _ExitTree()
    {
        if (instance == this) instance = null;
    }
}