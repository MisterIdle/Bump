using Godot;
using System;

public static class Time
{
    private static float targetTimeScale = 1f;
    private static float currentTimeScale = 1f;
    private static float lerpSpeed = 5f;

    private static Timer slowMotionTimer = null;

    public static float GetDelta(float pDelta)
    {
        Update(pDelta);
        return pDelta * currentTimeScale;
    }

    public static void Update(float delta)
    {
        currentTimeScale = Mathf.Lerp(currentTimeScale, targetTimeScale, lerpSpeed * delta);
    }

    public static void SlowMotion(float pTime, float pDuration = 0f)
    {
        targetTimeScale = Mathf.Clamp(pTime, 0f, 1f);

        if (pDuration > 0f)
        {
            if (slowMotionTimer == null)
            {
                slowMotionTimer = new Timer();
                slowMotionTimer.OneShot = true;
                slowMotionTimer.Timeout += ResetTimeScale;
                GameManager.GetInstance().AddChild(slowMotionTimer);
            }

            slowMotionTimer.WaitTime = pDuration;
            slowMotionTimer.Start();
        }
    }

    public static void ResetTimeScale()
    {
        targetTimeScale = 1f;
    }
}
