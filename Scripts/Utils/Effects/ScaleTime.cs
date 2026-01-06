using Godot;
using System;

using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NToolsBox;

public static class ScaleTime
{
    private static float targetTimeScale = 1f;
    public static float currentTimeScale = 1f;
    private static float lerpSpeed = 5f;
    private static float maxFrameTime = 0.05f;

    // ========================================================================
    // UPDATE
    // ========================================================================

    public static void Update(float pDelta)
    {
        currentTimeScale = Mathf.Lerp(currentTimeScale, targetTimeScale, lerpSpeed * pDelta);
    }

    public static float GetDelta(float pDelta)
    {
        Update(pDelta);

        float safeDelta = Mathf.Min(pDelta, maxFrameTime);
        return safeDelta * currentTimeScale;
    }

    // ========================================================================
    // EFFECTS
    // ========================================================================

    public static void SlowMotion(float pScale = 0.01f, float pDuration = 0.3f)
    {
        targetTimeScale = pScale;

        Timer lTimer = new Timer();

        lTimer.OneShot = true;
        lTimer.WaitTime = pDuration;

        lTimer.Timeout += () =>
        {
            ResetTimeScale();
            lTimer.QueueFree();
        };

        GameManager.GetInstance().gameContainer.AddChild(lTimer);
        lTimer.Start();
    }

    public static void ResetTimeScale()
    {
        targetTimeScale = 1f;
    }
}