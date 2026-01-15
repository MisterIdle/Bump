using Com.IsartDigital.Shmup.NHUD;
using Godot;
using System;

public partial class Intro : Control
{
    [ExportCategory("References")]
    [Export] public TextureRect LogoDisplay;
    [Export] public Texture2D[] LogoList;
    [Export] public PackedScene GameScene;

    [ExportCategory("Timing")]
    [Export] public float FallDuration = 0.5f;
    [Export] public float ImpactSquashDuration = 0.1f;
    [Export] public float BumpSettleDuration = 0.6f;
    [Export] public float PauseDuration = 0.3f;
    [Export] public float ExitDuration = 0.3f;

    [ExportCategory("Settings")]
    [Export] public float DropHeightOffset = 300f;
    [Export] public Vector2 SquashScale = new Vector2(1.6f, 0.4f);
    [Export] public float Elasticity = 2.0f;

    public override void _Ready()
    {
        Vector2 viewportSize = GetViewportRect().Size;
        Vector2 centerPos = (viewportSize / 2) - (LogoDisplay.Size / 2);
        Vector2 startPos = new Vector2(centerPos.X, -LogoDisplay.Size.Y - DropHeightOffset);

        LogoDisplay.PivotOffset = LogoDisplay.Size / 2;

        Tween tween = CreateTween();

        foreach (Texture2D tex in LogoList)
        {
            tween.TweenCallback(Callable.From(() =>
            {
                LogoDisplay.Texture = tex;
                LogoDisplay.Position = startPos;
                LogoDisplay.Scale = Vector2.One;
                LogoDisplay.Visible = true;
            }));

            tween.TweenProperty(LogoDisplay, "position", centerPos, FallDuration)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.In);

            tween.TweenProperty(LogoDisplay, "scale", SquashScale, ImpactSquashDuration)
                .SetTrans(Tween.TransitionType.Expo)
                .SetEase(Tween.EaseType.Out);

            tween.TweenProperty(LogoDisplay, "scale", Vector2.One, BumpSettleDuration)
                .SetTrans(Tween.TransitionType.Elastic)
                .SetEase(Tween.EaseType.Out);

            tween.TweenInterval(PauseDuration);

            tween.TweenProperty(LogoDisplay, "position", startPos, ExitDuration)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.In);
        }

        tween.TweenCallback(Callable.From(LoadNextScene));
    }

    public override void _Input(InputEvent pEvent)
    {
        if (!Visible) return;

        bool lIsAnyAction = pEvent.IsPressed() &&
                           (pEvent is InputEventKey ||
                            pEvent is InputEventMouseButton ||
                            pEvent is InputEventJoypadButton);

        if (lIsAnyAction)
        {
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        if (GameScene != null) GetTree().ChangeSceneToPacked(GameScene);
    }
}