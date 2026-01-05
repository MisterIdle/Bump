using Godot;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;

public partial class Spiko : Spike
{
    [Export] public float trackingTime = 1.0f;

    private Timer trackingTimer;
    private bool isTracking = true;
    private bool hasTimerStarted = false;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void Initiate()
    {
        base.Initiate();
        isTracking = true;
        hasTimerStarted = false;
        SetupTimer();
    }

    private void SetupTimer()
    {
        if (trackingTimer == null)
        {
            trackingTimer = new Timer();
            trackingTimer.OneShot = true;
            trackingTimer.Timeout += OnTrackingTimeout;
            AddChild(trackingTimer);
        }

        trackingTimer.WaitTime = trackingTime;
    }

    private void OnTrackingTimeout()
    {
        isTracking = false;
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    protected override void Update(float pDelta)
    {
        if (!isActive) return;

        if (!hasTimerStarted)
        {
            trackingTimer.Start();
            hasTimerStarted = true;
        }

        if (isTracking)
        {
            base.Update(pDelta);
        }
        else
        {
            GlobalPosition += velocity * speed * pDelta;
        }
    }
}