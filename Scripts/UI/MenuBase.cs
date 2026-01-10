using Godot;

using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NHUD;

public partial class MenuBase : Control
{
    [ExportCategory("Base Animation")]
    [Export] protected float animDurationEnter = 0.5f;
    [Export] protected float animDurationExit = 0.2f;
    [Export] protected float animStagger = 0.08f;
    [Export] protected float animInitialDelay = 0.2f;

    private Tween animationTween;

    private const float HALF_DIVISOR = 2f;
    private const float ALPHA_FADE_DURATION = 0.3f;
    private const float EXIT_ALPHA_MULTIPLIER = 0.6f;
    private const float ALPHA_OPAQUE = 1f;
    private const float ALPHA_TRANSPARENT = 0f;
    private const float NO_DELAY = 0f;

    // ========================================================================
    // UPDATE
    // ========================================================================

    public virtual void OnEnter()
    {
        Visible = true;
        ResetControlsRecursive(this);
        PlayGenericEntryAnimation();
    }

    public virtual void OnExit() { }

    // ========================================================================
    // ANIMATION
    // ========================================================================

    protected void ResetControlsRecursive(Node pNode)
    {
        foreach (Node lChild in pNode.GetChildren())
        {
            if (lChild is Control lControl)
            {
                if (IsAnimatable(lControl))
                {
                    lControl.Scale = Vector2.Zero;
                    lControl.Modulate = Colors.Transparent;
                    lControl.PivotOffset = lControl.Size / HALF_DIVISOR;
                }

                if (lChild.GetChildCount() > 0)
                    ResetControlsRecursive(lChild);
            }
        }
    }

    protected void PlayGenericEntryAnimation()
    {
        if (animationTween != null && animationTween.IsValid()) animationTween.Kill();

        animationTween = CreateTween();
        animationTween.SetParallel(true);

        float lDelay = animInitialDelay;
        AnimateControlsRecursive(this, animationTween, lDelay);

        animationTween.Finished += () => MenuManager.GetInstance().loading = false;
    }

    private void AnimateControlsRecursive(Node pNode, Tween pTween, float pDelay)
    {
        foreach (Node lChild in pNode.GetChildren())
        {
            if (lChild is Control lControl)
            {
                if (IsAnimatable(lControl))
                {
                    pTween.TweenProperty(lControl, (string)PropertyName.Modulate + ":a", ALPHA_OPAQUE, ALPHA_FADE_DURATION)
                        .SetDelay(pDelay);

                    pTween.TweenProperty(lControl, (string)PropertyName.Scale, Vector2.One, animDurationEnter)
                        .SetDelay(pDelay)
                        .SetEase(Tween.EaseType.Out)
                        .SetTrans(Tween.TransitionType.Back);

                    pDelay += animStagger;
                }

                if (lChild.GetChildCount() > 0)
                    AnimateControlsRecursive(lChild, pTween, pDelay);
            }
        }
    }

    public Tween PlayGenericExitAnimation()
    {
        if (animationTween != null && animationTween.IsValid()) animationTween.Kill();

        animationTween = CreateTween();
        animationTween.SetParallel(true);

        float lDelay = NO_DELAY;
        AnimateControlsExitRecursive(this, animationTween, lDelay);

        animationTween.Chain().TweenCallback(Callable.From(() => Visible = false));

        return animationTween;
    }

    private void AnimateControlsExitRecursive(Node pNode, Tween pTween, float pDelay)
    {
        foreach (Node lChild in pNode.GetChildren())
        {
            if (lChild is Control lControl)
            {
                if (IsAnimatable(lControl))
                {
                    pTween.TweenProperty(lControl, (string)PropertyName.Modulate + ":a", ALPHA_TRANSPARENT, animDurationExit * EXIT_ALPHA_MULTIPLIER)
                        .SetDelay(pDelay);

                    pTween.TweenProperty(lControl, (string)PropertyName.Scale, Vector2.Zero, animDurationExit)
                        .SetDelay(pDelay)
                        .SetEase(Tween.EaseType.In)
                        .SetTrans(Tween.TransitionType.Back);

                    pDelay += animStagger;
                }

                if (lChild.GetChildCount() > 0)
                    AnimateControlsExitRecursive(lChild, pTween, pDelay);
            }
        }
    }

    private bool IsAnimatable(Control pControl)
    {
        return pControl is Button ||
               pControl is Label ||
               pControl is CheckBox ||
               pControl is HSlider ||
               pControl is TextureRect ||
               pControl is ProgressBar;
    }
}