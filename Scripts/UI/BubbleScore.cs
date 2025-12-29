using Godot;
using System;

public partial class BubbleScore : Node2D
{
    public int score;
    [Export] private Label label;

    public override void _Ready()
    {
        base._Ready();

        label.Text = score.ToString();

        AnimateFloatingText();
    }

    private void AnimateFloatingText()
    {
        HUD.GetInstance().AnimatedScore(score);

        Tween lTween = CreateTween();

        lTween.TweenProperty(this, (string)PropertyName.Position + ":y", -150.0f, 1.0f)
             .AsRelative()
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.Out);

        lTween.Parallel().TweenProperty(this, (string)PropertyName.Modulate, new Color(0,0,0,0), 1.0f)
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.In);

        lTween.Finished += FinishAction;
    }

    private void FinishAction()
    {
        QueueFree();
    }
}