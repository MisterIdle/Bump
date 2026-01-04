using Godot;

public partial class Rushroops : ShelledEnemy
{
    [ExportGroup("Rushroops Specifics")]
    [Export] private float chargeWait = 15f;
    [Export] private float chargeSpeed = 300f;
    [Export] private float recoilDistance = -25f;
    [Export] private float recoilDuration = 1f;
    [Export] private Vector2 basePosShell;

    private Timer chargeTimer;
    private bool isCharging = false;

    public override void Initiate()
    {
        base.Initiate();

        chargeTimer = new Timer();
        AddChild(chargeTimer);
        chargeTimer.WaitTime = chargeWait;
        chargeTimer.Autostart = true;
        chargeTimer.Timeout += StartCharge;
        chargeTimer.Start();

        shell.SetShellColor(Colors.Blue);

        StartSpikeRoutine();
    }

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        if (!isCharging)
            velocity = new Vector2(angleX, angleY);
    }

    private void StartCharge()
    {
        if (!isCharging) 
            isCharging = true;

        chargeTimer.Stop();
        StopSpikeRoutine();
        chargeTimer.Stop();

        shellProjectileProtect = true;
        facePlayer = false;
        wing.SetSpeedDouble(true);

        Effect.SquashStretch(this, baseScale, false);

        Vector2 lDirection = playerDirection.Normalized();
        Vector2 lRecoil = Position - lDirection * recoilDistance;

        speed = chargeSpeed;
        velocity = lDirection;

        Tween lTween = CreateTween();
        lTween.Parallel().TweenProperty(this, "position", lRecoil, recoilDuration)
             .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        lTween.Parallel().TweenProperty(shell, "position", basePosShell, recoilDuration)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
    }

    public override void EntityDeath()
    {
        base.EntityDeath();
        chargeTimer.Stop();
    }
}
