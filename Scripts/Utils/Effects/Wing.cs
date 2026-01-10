using Godot;
using System;

namespace Com.IsartDigital.Shmup.NToolsBox;

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

    private const float MIN_SPEED_SCALE = 0.1f;
    private const float DOUBLE_SPEED_MULTIPLIER = 2.0f;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        if (speedScale <= 0) 
            speedScale = MIN_SPEED_SCALE;
        StartAnimations();
    }

    // ========================================================================
    // ANIMATION
    // ========================================================================

    public void SetSpeedDouble(bool pOn)
    {
        speedDouble = pOn;
        StartAnimations();
    }

    private void StartAnimations()
    {
        if (rotationTween != null && rotationTween.IsValid()) rotationTween.Kill();
        if (skewTween != null && skewTween.IsValid()) skewTween.Kill();

        float lFinalDurationDown = baseDurationDown / speedScale;
        float lFinalDurationUp = baseDurationUp / speedScale;

        if (speedDouble)
        {
            lFinalDurationDown /= DOUBLE_SPEED_MULTIPLIER;
            lFinalDurationUp /= DOUBLE_SPEED_MULTIPLIER;
        }

        AnimateRotation(lFinalDurationDown, lFinalDurationUp);
        AnimateSkew(lFinalDurationDown, lFinalDurationUp);
    }

    private void AnimateRotation(float pTimeDown, float pTimeUp)
    {
        rotationTween = CreateTween();
        rotationTween.SetLoops();

        rotationTween.TweenProperty(this, (string)PropertyName.RotationDegrees, rotDown, pTimeDown)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);

        rotationTween.TweenProperty(this, (string)PropertyName.RotationDegrees, rotUp, pTimeUp)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
    }

    private void AnimateSkew(float pTimeDown, float pTimeUp)
    {
        skewTween = CreateTween();
        skewTween.SetLoops();

        skewTween.TweenProperty(this, (string)PropertyName.Skew, Mathf.DegToRad(skewDown), pTimeDown)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);

        skewTween.TweenProperty(this, (string)PropertyName.Skew, Mathf.DegToRad(skewUp), pTimeUp)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
    }
}