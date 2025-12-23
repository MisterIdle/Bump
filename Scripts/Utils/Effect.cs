using Godot;

public static class Effect
{
    private static string scale = "scale";
    private static string modulate = "modulate";

    public static void BumpScale(Node2D pWho, float pScaleBump = 1.2f, float pDuration = 0.1f)
    {
        Tween lTween = pWho.CreateTween();

        lTween.TweenProperty(pWho, scale, pWho.Scale * pScaleBump, pDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        lTween.TweenProperty(pWho, scale, Vector2.One, pDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
    }

    public static Tween FlashEntityColor(Node2D pWho, Color pColor, float pDuration = 0.1f)
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
}
