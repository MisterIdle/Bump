using Godot;

namespace Com.IsartDigital.Shmup.NHUD;

public partial class MenuBase : Control
{
    [ExportCategory("Base Animation")]
    [Export] protected float animDurationEnter = 0.5f;
    [Export] protected float animDurationExit = 0.2f;
    [Export] protected float animStagger = 0.08f;
    [Export] protected float animInitialDelay = 0.2f;

    private Tween animationTween;

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
                    lControl.Modulate = new Color(1, 1, 1, 0);
                    lControl.PivotOffset = lControl.Size / 2;
                }

                if (lChild.GetChildCount() > 0)
                    ResetControlsRecursive(lChild);
            }
        }
    }

    protected void PlayGenericEntryAnimation()
    {
        animationTween?.Kill();
        animationTween = CreateTween();
        animationTween.SetParallel(true);

        float lDelay = animInitialDelay;
        AnimateControlsRecursive(this, animationTween, lDelay);
    }

    private void AnimateControlsRecursive(Node pNode, Tween pTween, float pDelay)
    {
        foreach (Node lChild in pNode.GetChildren())
        {
            if (lChild is Control lControl)
            {
                if (IsAnimatable(lControl))
                {
                    pTween.TweenProperty(lControl, "modulate:a", 1f, 0.3f)
                        .SetDelay(pDelay);

                    pTween.TweenProperty(lControl, "scale", Vector2.One, animDurationEnter)
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
        animationTween?.Kill();

        animationTween = CreateTween();
        animationTween.SetParallel(true);

        float lDelay = 0f;
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
                    pTween.TweenProperty(lControl, "modulate:a", 0f, animDurationExit * 0.6f)
                        .SetDelay(pDelay);

                    pTween.TweenProperty(lControl, "scale", Vector2.Zero, animDurationExit)
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
        return pControl is Button || pControl is Label || pControl is CheckBox || pControl is HSlider || pControl is TextureRect || pControl is ProgressBar;
    }
}