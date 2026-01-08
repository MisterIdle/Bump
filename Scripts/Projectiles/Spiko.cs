using Godot;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;

public partial class Spiko : Spike
{
    [ExportGroup("Tracking Settings")]
    [Export] public float trackingTime = 1.5f;

    [ExportGroup("Dash Settings")]
    [Export] public float dashSpeedMultiplier = 3.0f;
    [Export] public Color lockedColor = Colors.OrangeRed;

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

        Modulate = Colors.White;

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
        SelfModulate = lockedColor;

        speed *= dashSpeedMultiplier;
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
            base.Update(pDelta);
        else
        {
            velocity = Vector2.FromAngle(Rotation);
            GlobalPosition += velocity * speed * pDelta;
        }
    }
}