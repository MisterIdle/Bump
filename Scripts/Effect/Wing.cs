using Godot;
using System;

public partial class Wing : Sprite2D
{
    public override void _Ready()
    {
        Tween lTween = CreateTween();

        lTween.SetLoops();

        lTween.TweenProperty(this, Property.TWEEN_ROTATION, 60, 1)
            .SetTrans(Tween.TransitionType.Circ)
            .SetEase(Tween.EaseType.InOut);

        lTween.TweenProperty(this, Property.TWEEN_ROTATION, -30, 1)
            .SetTrans(Tween.TransitionType.Circ)
            .SetEase(Tween.EaseType.InOut);
    }
}
