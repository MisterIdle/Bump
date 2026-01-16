using Godot;
using System;

namespace Com.IsartDigital.Shmup.NHUD;

public partial class Blackbar : Control
{
    private static Blackbar instance;
    public static Blackbar GetInstance() => instance;

    [Export] private ColorRect topBar;
    [Export] private ColorRect bottomBar;
    [Export] private float animDuration = 0.6f;
    [Export] private float barHeightRatio = 0.12f;

    private Tween tween;

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else { QueueFree(); return; }

        CallDeferred(nameof(InitPositions));
    }

    private void InitPositions()
    {
        float lScreenHeight = GetViewportRect().Size.Y;
        float lBarHeight = lScreenHeight * barHeightRatio;
        float lScreenWidth = GetViewportRect().Size.X;

        topBar.Size = new Vector2(lScreenWidth, lBarHeight);
        bottomBar.Size = new Vector2(lScreenWidth, lBarHeight);

        topBar.Position = new Vector2(0, -lBarHeight);
        bottomBar.Position = new Vector2(0, lScreenHeight);
    }

    public new void Show()
    {
        float lScreenHeight = GetViewportRect().Size.Y;
        float lBarHeight = lScreenHeight * barHeightRatio;
        float lScreenWidth = GetViewportRect().Size.X;

        topBar.Size = new Vector2(lScreenWidth, lBarHeight);
        bottomBar.Size = new Vector2(lScreenWidth, lBarHeight);

        float lTargetTop = 0f;
        float lTargetBottom = lScreenHeight - lBarHeight;

        RunAnimation(lTargetTop, lTargetBottom, Tween.EaseType.Out);
    }

    public new void Hide()
    {
        float lScreenHeight = GetViewportRect().Size.Y;
        float lBarHeight = lScreenHeight * barHeightRatio;

        float lTargetTop = -lBarHeight;
        float lTargetBottom = lScreenHeight;

        RunAnimation(lTargetTop, lTargetBottom, Tween.EaseType.In);
    }

    private void RunAnimation(float pTopY, float pBottomY, Tween.EaseType pEase)
    {
        tween?.Kill();
        tween = CreateTween();
        tween.SetParallel(true);
        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.SetEase(pEase);

        tween.TweenProperty(topBar, "position:y", pTopY, animDuration);
        tween.TweenProperty(bottomBar, "position:y", pBottomY, animDuration);
    }

    public override void _ExitTree()
    {
        if (instance == this) instance = null;
    }
}