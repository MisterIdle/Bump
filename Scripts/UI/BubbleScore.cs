using Godot;
using System;

using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NHUD;

public partial class BubbleScore : Node2D
{
    public int score;

    [ExportGroup("Nodes")]
    [Export] public Label label;

    [ExportGroup("Settings")]
    [Export] private float animationDuration = 1.0f;
    [Export] private float minRiseDistance = -150.0f;
    [Export] private float maxRiseDistance = -250.0f;
    [Export] private float maxRotationRange = 15.0f;
    [Export] private Vector2 offsetRange = new Vector2(20f, 10f);
    private Color fadeColor = new Color(1, 1, 1, 0);

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
        
        label.Text = score.ToString();

        AnimateFloatingText();
    }

    // ========================================================================
    // ANIMATION
    // ========================================================================

    private void AnimateFloatingText()
    {
        HUD.GetInstance().AnimatedScore(score);

        Tween lFloatingTween = CreateTween();

        RotationDegrees = GameManager.rand.RandfRange(-maxRotationRange, maxRotationRange);

        float lRandomRise = GameManager.rand.RandfRange(minRiseDistance, maxRiseDistance);

        lFloatingTween.TweenProperty(this, (string)PropertyName.Position + ":y", lRandomRise, animationDuration)
             .AsRelative()
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.Out);

        lFloatingTween.Parallel().TweenProperty(this, (string)PropertyName.Modulate, fadeColor, animationDuration)
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.In);

        lFloatingTween.Finished += FinishAction;
    }

    private void FinishAction()
    {
        QueueFree();
    }
}