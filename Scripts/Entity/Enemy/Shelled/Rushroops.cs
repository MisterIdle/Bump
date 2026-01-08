using Godot;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class Rushroops : ShelledEnemy
{
    [ExportGroup("Rushroops Specifics")]
    [Export] private float chargeWait = 5f;
    [Export] private float chargeDuration = 1.5f;
    [Export] private float chargeSpeed = 600f;
    [Export] private float recoilDistance = -25f;
    [Export] private float recoilDuration = 1f;
    [Export] private Vector2 basePosShell;

    private Timer chargeTimer;
    private bool isCharging = false;
    private float normalSpeed;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void Initiate()
    {
        base.Initiate();

        normalSpeed = speed;

        chargeTimer = new Timer();
        chargeTimer.WaitTime = chargeWait;
        chargeTimer.OneShot = true;
        chargeTimer.Timeout += StartCharge;

        AddChild(chargeTimer);
        chargeTimer.Start();

        shell?.SetShellColor(Colors.Blue);

        StartSpike();
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        if (!isCharging)
            velocity.X = angleX;
    }

    // ========================================================================
    // ATTACK (CHARGE LOOP)
    // ========================================================================

    private void StartCharge()
    {
        if (isCharging || isDeath) return;

        isCharging = true;

        StopSpike();

        shellProjectileProtect = true;
        wing?.SetSpeedDouble(true);

        Effect.SquashStretch(this, baseScale, false);

        Vector2 lRecoil = GlobalPosition - playerDirection * recoilDistance;

        Tween lTween = CreateTween();

        lTween.Parallel().TweenProperty(this, (string)PropertyName.GlobalPosition, lRecoil, recoilDuration)
             .SetTrans(Tween.TransitionType.Back)
             .SetEase(Tween.EaseType.Out);

        lTween.Parallel().TweenProperty(shell, (string)PropertyName.Position, basePosShell, recoilDuration)
           .SetTrans(Tween.TransitionType.Back)
           .SetEase(Tween.EaseType.Out);

        lTween.Finished += () =>
        {
            if (isDeath) return;

            facePlayer = false;

            speed = chargeSpeed;
            velocity = playerDirection;

            GetTree().CreateTimer(chargeDuration).Timeout += StopCharge;
        };
    }

    private void StopCharge()
    {
        if (isDeath) return;

        isCharging = false;

        speed = normalSpeed;
        velocity = Vector2.Zero;
        facePlayer = true;

        shellProjectileProtect = false;
        wing?.SetSpeedDouble(false);

        StartSpike();

        chargeTimer.Start(chargeWait);
    }

    // ========================================================================
    // COLLIDE
    // ========================================================================

    public override void EntityDeath()
    {
        chargeTimer?.Stop();
        base.EntityDeath();
    }
}