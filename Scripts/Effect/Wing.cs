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

        lTween.TweenProperty(this, Property.TWEEN_ROTATION_DEGREES, rotA, rotSpeedA)
            .SetTrans(Tween.TransitionType.Circ)
            .SetEase(Tween.EaseType.InOut);

        lTween.TweenProperty(this, Property.TWEEN_ROTATION_DEGREES, rotB, rotSpeedB)
            .SetTrans(Tween.TransitionType.Circ)
            .SetEase(Tween.EaseType.InOut);
    }
}
