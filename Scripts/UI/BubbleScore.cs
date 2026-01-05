using Godot;
using System;

using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NHUD;

public partial class BubbleScore : Node2D
{
    public int score;

    [ExportGroup("Nodes")]
    [Export] private Label label;

    [ExportGroup("Settings")]
    [Export] private float animationDuration = 1.0f;
    [Export] private float minRiseDistance = -150.0f;
    [Export] private float maxRiseDistance = -250.0f;
    [Export] private float maxRotationRange = 15.0f;
    [Export] private Vector2 offsetRange = new Vector2(20f, 10f);

    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        Vector2 lOffset = new Vector2(
            GameManager.rand.RandfRange(-offsetRange.X, offsetRange.X),
            GameManager.rand.RandfRange(-offsetRange.Y, offsetRange.Y)
        );

        Position += lOffset;

        if (label != null)
            label.Text = score.ToString();

        AnimateFloatingText();
    }

    // ========================================================================
    // ANIMATION
    // ========================================================================

    private void AnimateFloatingText()
    {
        HUD.GetInstance().AnimatedScore(score);

        Tween lTween = CreateTween();

        RotationDegrees = GameManager.rand.RandfRange(-maxRotationRange, maxRotationRange);

        float lRandomRise = GameManager.rand.RandfRange(minRiseDistance, maxRiseDistance);

        lTween.TweenProperty(this, (string)PropertyName.Position + ":y", lRandomRise, animationDuration)
             .AsRelative()
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.Out);

        lTween.Parallel().TweenProperty(this, (string)PropertyName.Modulate, new Color(1, 1, 1, 0), animationDuration)
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.In);

        lTween.Finished += FinishAction;
    }

    private void FinishAction()
    {
        QueueFree();
    }
}