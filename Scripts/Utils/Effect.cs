using Godot;

public static class Effect
{
    private static string scale = "scale";
    private static string modulate = "modulate";

    public static void BumpScale(Node2D pWho, float pScaleBump = 1.2f, float pDuration = 0.1f)
    {
        Tween lTween = pWho.CreateTween();

        Vector2 pBaseScale = pWho.Scale;

        lTween.TweenProperty(pWho, scale, pBaseScale * pScaleBump, pDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        lTween.TweenProperty(pWho, scale, pBaseScale, pDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
    }

    public static Tween FlashColor(Node2D pWho, Color pColor, float pDuration = 0.1f)
    {
        Tween lTween = pWho.CreateTween();

        lTween.TweenProperty(pWho, modulate, pColor, pDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        lTween.TweenProperty(pWho, modulate, Colors.White, pDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        return lTween;
    }

    public static void BumpHUD(Control pWho, float pScaleBump = 1.2f, float pDuration = 0.1f)
    {
        if (pWho == null) return;

        Tween lTween = pWho.CreateTween();

        lTween.TweenProperty(pWho, scale, pWho.Scale * pScaleBump, pDuration)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);

        lTween.TweenProperty(pWho, scale, pWho.Scale, pDuration)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);
    }

    public static Tween SquashStretch(Node2D pWho, bool pOnX, bool pStaySquashed = false, float pSquashScale = 0.4f, float pStretchScale = 1.2f, float pDuration = 0.2f)
    {
        Tween lTween = pWho.CreateTween();

        // Si spam le rend tout petit biug chelou
        Vector2 lBaseScale = pWho.Scale;

        GD.Print(lBaseScale);

        Vector2 lSquash = lBaseScale;
        Vector2 lStretch = lBaseScale;

        if (pOnX)
        {
            lSquash.X *= pSquashScale;
            lStretch.X *= pStretchScale;
        }
        else
        {
            lSquash.Y *= pSquashScale;
            lStretch.Y *= pStretchScale;
        }

        lTween.TweenProperty(pWho, scale, lSquash, pDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        if (!pStaySquashed)
        {
            lTween.TweenProperty(pWho, scale, lStretch, pDuration)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);

            lTween.TweenProperty(pWho, scale, lBaseScale, pDuration)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
        }

        return lTween;
    }
}
