using Godot;
using System;

public partial class Wing : Sprite2D
{
    [Export] private float rotA;
    [Export] private float rotB;

    [Export] private float rotSpeedA;
    [Export] private float rotSpeedB;

    public override void _Ready()
    {
        Tween lTween = CreateTween();
        lTween.SetLoops();

        lTween.TweenProperty(this, (string)PropertyName.RotationDegrees, rotA, rotSpeedA)
            .SetTrans(Tween.TransitionType.Circ)
            .SetEase(Tween.EaseType.InOut);

        lTween.TweenProperty(this, (string)PropertyName.RotationDegrees, rotB, rotSpeedB)
            .SetTrans(Tween.TransitionType.Circ)
            .SetEase(Tween.EaseType.InOut);
    }
}
