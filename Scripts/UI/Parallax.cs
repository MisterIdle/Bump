using Godot;
using System;
using System.Collections.Generic;

public partial class Parallax : ParallaxBackground
{
    [Export] public float SpawnY = 1500f;
    [Export] public float Duration = 1.5f;
    [Export] public float Stagger = 0.2f;

    public override void _Ready()
    {
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
}