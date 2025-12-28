using Godot;
using System;

public partial class Wing : Sprite2D
{
    [Export] private float speedScale = 1.0f;

    [Export] private float rotDown = 30f;
    [Export] private float skewDown = 50f;
    [Export] private float baseDurationDown = 0.4f;

    [Export] private float rotUp = 0f;
    [Export] private float skewUp = 0f;
    [Export] private float baseDurationUp = 0.6f;

    [Export] private bool speedDouble = false;

    private Tween rotationTween;
    private Tween skewTween;

    public override void _Ready()
    {
        if (speedScale <= 0) speedScale = 0.1f;
        StartAnimations();
    }

    public void SetSpeedDouble(bool pOn)
    {
        speedDouble = pOn;
        StartAnimations();
    }

    private void StartAnimations()
    {
        rotationTween?.Kill();
        skewTween?.Kill();

        float finalDurationDown = baseDurationDown / speedScale;
        float finalDurationUp = baseDurationUp / speedScale;

        if (speedDouble)
        {
            finalDurationDown /= 2f;
            finalDurationUp /= 2f;
        }

        AnimateRotation(finalDurationDown, finalDurationUp);
        AnimateSkew(finalDurationDown, finalDurationUp);
    }

    private void AnimateRotation(float timeDown, float timeUp)
    {
        rotationTween = CreateTween();
        rotationTween.SetLoops();

        rotationTween.TweenProperty(this, (string)PropertyName.RotationDegrees, rotDown, timeDown)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);

        rotationTween.TweenProperty(this, (string)PropertyName.RotationDegrees, rotUp, timeUp)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
    }

    private void AnimateSkew(float timeDown, float timeUp)
    {
        skewTween = CreateTween();
        skewTween.SetLoops();

        skewTween.TweenProperty(this, (string)PropertyName.Skew, Mathf.DegToRad(skewDown), timeDown)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);

        skewTween.TweenProperty(this, (string)PropertyName.Skew, Mathf.DegToRad(skewUp), timeUp)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
    }
}
