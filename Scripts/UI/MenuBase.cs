using Godot;
using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NHUD;

public partial class MenuBase : Control
{
    [ExportCategory("Base Animation")]
    [Export] protected float animDurationEnter = 0.5f;
    [Export] protected float animDurationExit = 0.2f;
    [Export] protected float animStagger = 0.05f;
    [Export] protected float animInitialDelay = 0.1f;

    [Export] protected bool animateRootScale = false;

    private Tween animationTween;

    private const float HALF_DIVISOR = 2f;
    private const float ALPHA_FADE_DURATION = 0.3f;
    private const float EXIT_ALPHA_MULTIPLIER = 0.6f;
    private const float ALPHA_OPAQUE = 1f;
    private const float ALPHA_TRANSPARENT = 0f;

    public virtual void OnEnter()
    {
        Visible = true;

        ResetSingleControl(this);
        ResetControlsRecursive(this);

        PlayGenericEntryAnimation();
    }

    public virtual void OnExit() { }

    // ========================================================================
    // LOGIQUE ATOMIQUE
    // ========================================================================

    private void ResetSingleControl(Control pControl)
    {
        if (!IsAnimatable(pControl)) return;

        pControl.Modulate = Colors.Transparent;
        pControl.PivotOffset = pControl.Size / HALF_DIVISOR;

        if (pControl != this || animateRootScale)
        {
            pControl.Scale = Vector2.Zero;
        }
    }

    private void AnimateSingleControlEntry(Control pControl, Tween pTween, float pDelay)
    {
        if (!IsAnimatable(pControl)) return;

        pTween.TweenProperty(pControl, (string)PropertyName.Modulate + ":a", ALPHA_OPAQUE, ALPHA_FADE_DURATION)
              .SetDelay(pDelay);

        if (pControl != this || animateRootScale)
        {
            pTween.TweenProperty(pControl, (string)PropertyName.Scale, Vector2.One, animDurationEnter)
                  .SetDelay(pDelay)
                  .SetEase(Tween.EaseType.Out)
                  .SetTrans(Tween.TransitionType.Back);
        }
    }

    private void AnimateSingleControlExit(Control pControl, Tween pTween, float pDelay)
    {
        if (!IsAnimatable(pControl)) return;

        pTween.TweenProperty(pControl, (string)PropertyName.Modulate + ":a", ALPHA_TRANSPARENT, animDurationExit * EXIT_ALPHA_MULTIPLIER)
              .SetDelay(pDelay);

        if (pControl != this || animateRootScale)
        {
            pTween.TweenProperty(pControl, (string)PropertyName.Scale, Vector2.Zero, animDurationExit)
                  .SetDelay(pDelay)
                  .SetEase(Tween.EaseType.In)
                  .SetTrans(Tween.TransitionType.Back);
        }
    }

    // ========================================================================
    // RECURSION
    // ========================================================================

    protected void ResetControlsRecursive(Node pNode)
    {
        foreach (Node lChild in pNode.GetChildren())
        {
            if (lChild is Control lControl)
            {
                ResetSingleControl(lControl);
                if (lChild.GetChildCount() > 0) ResetControlsRecursive(lChild);
            }
        }
    }

    protected void PlayGenericEntryAnimation()
    {
        if (animationTween != null && animationTween.IsValid()) animationTween.Kill();

        animationTween = CreateTween();
        animationTween.SetParallel(true);

        AnimateSingleControlEntry(this, animationTween, 0);
        AnimateControlsRecursive(this, animationTween, animInitialDelay);

        animationTween.Finished += () => MenuManager.GetInstance().loading = false;
    }

    private void AnimateControlsRecursive(Node pNode, Tween pTween, float pCurrentBaseDelay)
    {
        float lSiblingDelay = pCurrentBaseDelay;

        foreach (Node lChild in pNode.GetChildren())
        {
            if (lChild is Control lControl)
            {
                if (IsAnimatable(lControl))
                {
                    AnimateSingleControlEntry(lControl, pTween, lSiblingDelay);

                    lSiblingDelay += animStagger;
                }

                if (lChild.GetChildCount() > 0)
                {
                    AnimateControlsRecursive(lChild, pTween, lSiblingDelay);
                }
            }
        }
    }

    public Tween PlayGenericExitAnimation()
    {
        if (animationTween != null && animationTween.IsValid()) animationTween.Kill();

        animationTween = CreateTween();
        animationTween.SetParallel(true);

        AnimateControlsExitRecursive(this, animationTween, 0);
        AnimateSingleControlExit(this, animationTween, 0);

        animationTween.Chain().TweenCallback(Callable.From(() => Visible = false));
        return animationTween;
    }

    private void AnimateControlsExitRecursive(Node pNode, Tween pTween, float pDelay)
    {
        float lSiblingDelay = pDelay;

        foreach (Node lChild in pNode.GetChildren())
        {
            if (lChild is Control lControl)
            {
                if (IsAnimatable(lControl))
                {
                    AnimateSingleControlExit(lControl, pTween, lSiblingDelay);
                    lSiblingDelay += (animStagger / 2f);
                }

                if (lChild.GetChildCount() > 0)
                    AnimateControlsExitRecursive(lChild, pTween, lSiblingDelay);
            }
        }
    }

    private bool IsAnimatable(Control pControl)
    {
        if (pControl == this) return true;

        return pControl is Button ||
               pControl is Label ||
               pControl is CheckBox ||
               pControl is HSlider ||
               pControl is TextureRect ||
               pControl is ProgressBar;
    }
}