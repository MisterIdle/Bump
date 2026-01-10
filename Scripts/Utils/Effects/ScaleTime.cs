using Godot;
using System;

using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NToolsBox;

public static class ScaleTime
{
    private const float DEFAULT_TIME_SCALE = 1f;
    private const float MAX_FRAME_TIME = 0.05f;

    private const float DEFAULT_SLOWMO_FACTOR = 0.05f;
    private const float DEFAULT_SLOWMO_DURATION = 0.2f;

    private const float MIN_TIMESCALE = 0f;
    private const float MAX_TIMESCALE = 1f;

    public static float currentTimeScale = DEFAULT_TIME_SCALE;

    // ========================================================================
    // UPDATE
    // ========================================================================

    public static float GetDelta(float pDelta)
    {
        float lSafeDelta = Mathf.Min(pDelta, MAX_FRAME_TIME);
        return lSafeDelta * currentTimeScale;
    }

    // ========================================================================
    // EFFECTS
    // ========================================================================

    public static void SlowMotion(float pTime = DEFAULT_SLOWMO_FACTOR, float pDuration = DEFAULT_SLOWMO_DURATION)
    {
        currentTimeScale = Mathf.Clamp(pTime, MIN_TIMESCALE, MAX_TIMESCALE);

        Timer lTimer = new Timer();

        lTimer.OneShot = true;
        lTimer.WaitTime = pDuration;

        lTimer.Timeout += () =>
        {
            ResetTimeScale();
            lTimer.QueueFree();
        };

        GameManager lManager = GameManager.GetInstance();
        if (lManager != null && lManager.gameContainer != null)
        {
            lManager.gameContainer.AddChild(lTimer);
            lTimer.Start();
        }
        else
        {
            lTimer.QueueFree();
            ResetTimeScale();
        }
    }

    public static void ResetTimeScale()
    {
        currentTimeScale = DEFAULT_TIME_SCALE;
    }
}