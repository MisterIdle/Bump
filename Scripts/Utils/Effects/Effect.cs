using Godot;

using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity;

namespace Com.IsartDigital.Shmup.NToolsBox;

public static class Effect
{
    private static string scaleProperty = "scale";
    private static string modulateProperty = "modulate";
    private static string offsetProperty = "offset";

    public static Tween BumpScale(Node2D pWho, Vector2 pBaseScale, float pScaleBump = 1.2f, float pDuration = 0.1f)
    {
        pWho.Scale = pBaseScale;

        Tween lTween = pWho.CreateTween();

        lTween.TweenProperty(pWho, scaleProperty, pBaseScale * pScaleBump, pDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        lTween.TweenProperty(pWho, scaleProperty, pBaseScale, pDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        return lTween;
    }

    public static Tween FlashColor(Node2D pWho, Color pColor, float pDuration = 0.1f)
    {
        Tween lTween = pWho.CreateTween();

        lTween.TweenProperty(pWho, modulateProperty, pColor, pDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        lTween.TweenProperty(pWho, modulateProperty, Colors.White, pDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        return lTween;
    }

    public static void BumpHUD(Control pWho, Vector2 pBaseScale, float pScaleBump = 1.2f, float pDuration = 0.1f)
    {
        if (pWho == null) return;
    
        Tween lTween = pWho.CreateTween();
    
        pWho.Scale = pBaseScale;
    
        lTween.TweenProperty(pWho, scaleProperty, pWho.Scale * pScaleBump, pDuration)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);
    
        lTween.TweenProperty(pWho, scaleProperty, pWho.Scale, pDuration)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);
    }

    public static Tween SquashStretch(Node2D pWho, Vector2 pBaseScale, bool pOnX, bool pStaySquashed = false, float pSquashScale = 0.8f, float pStretchScale = 1.2f, float pDuration = 0.2f)
    {
        pWho.Scale = pBaseScale;

        Tween lTween = pWho.CreateTween();

        Vector2 lSquash = pBaseScale;
        Vector2 lStretch = pBaseScale;

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

        lTween.TweenProperty(pWho, scaleProperty, lSquash, pDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        if (!pStaySquashed)
        {
            lTween.TweenProperty(pWho, scaleProperty, lStretch, pDuration)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);

            lTween.TweenProperty(pWho, scaleProperty, pBaseScale, pDuration)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
        }

        return lTween;
    }

    public static void JoyVibration(float pTime, float pWeak = 0.3f, float pStrong = 0.3f, int pDevice = 0)
    {
        Input.StartJoyVibration(pDevice, pWeak, pStrong, pTime);
    }

    public static void Shake(float pForce, float pTime, float pX = 1, float pY = 1, float pStep = 0.025f)
    {
        Camera pSelf = Camera.GetInstance();
        pSelf.Offset = Vector2.Zero;

        int pSteps = Mathf.Max(1, (int)(pTime / pStep));
        float pStepDuration = pStep;


        Tween lTween = pSelf.CreateTween();

        for (int i = 0; i < pSteps; i++)
        {
            float lTime = 1f - (float)i / (pSteps - 1);

            Vector2 lOffset = new Vector2(GameManager.rand.RandfRange(-pX, pX), GameManager.rand.RandfRange(-pY, pY)) * pForce * lTime;

            lTween.TweenProperty(pSelf, offsetProperty, lOffset, pStepDuration)
                  .SetTrans(Tween.TransitionType.Sine)
                  .SetEase(Tween.EaseType.InOut);
        }

        lTween.TweenProperty(pSelf, offsetProperty, Vector2.Zero, pStepDuration)
              .SetTrans(Tween.TransitionType.Sine)
              .SetEase(Tween.EaseType.InOut);
    }
}
