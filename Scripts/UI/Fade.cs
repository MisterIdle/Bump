using Godot;

public partial class Fade : ColorRect
{
    [Export] public float Duration = 500f;
    [Export] public Color FadeColor = Colors.Black;

    public override void _Ready()
    {
        ZIndex = 4096;

        SetAnchorsPreset(LayoutPreset.FullRect);

        MouseFilter = MouseFilterEnum.Ignore;

        Color = FadeColor;

        Visible = true;

        Tween tween = CreateTween();

        tween.TweenProperty(this, "color:a", 0.0f, Duration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);

        tween.TweenCallback(Callable.From(QueueFree));
    }
}