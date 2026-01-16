using Godot;

public partial class Fade : ColorRect
{
    [Export] public float duration = 0.5f;
    [Export] public Color fadeColor = Colors.Black;

    public override void _EnterTree()
    {
        Color = fadeColor;
        Color = new Color(Color, 1f);
        Visible = true;
    }

    public override void _Ready()
    {
        var tween = CreateTween();

        tween.TweenProperty(this, "color:a", 0f, duration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);

        tween.TweenCallback(Callable.From(QueueFree));
    }
}
