using Godot;

public partial class Rushroops : ShelledEnemy
{
    [Export] private PackedScene shotScene;

    [Export] private float fireRate = 1.5f;
    [Export] private float rotationSpeed = 5f;

    [Export] private float chargeWait = 15f;
    [Export] private float chargeSpeed = 300f;
    [Export] private float recoilDistance = -25f;
    [Export] private float recoilDuration = 1f;
    [Export] private Vector2 basePosShell;

    private Timer shotTimer;
    private Timer chargeTimer;

    private bool isCharging = false;

    public override void Initiate()
    {
        base.Initiate();

        shotTimer = new Timer();
        AddChild(shotTimer);

        shotTimer.WaitTime = fireRate;
        shotTimer.Autostart = true;
        shotTimer.Timeout += Shoot;

        shotTimer.Start();

        chargeTimer = new Timer();
        AddChild(chargeTimer);

        chargeTimer.WaitTime = chargeWait;
        chargeTimer.Autostart = true;
        chargeTimer.Timeout += () => StartCharge(chargeTimer, shotTimer);

        chargeTimer.Start();

        shell.SetShellColor(Colors.Blue);
    }

    private void Shoot()
    {
        Shot lShot = Shot.Create(shotScene, GlobalPosition, playerDirection);

        Projectiles.enemyProjectilesList.Add(lShot);

        GameManager.GetInstance().gameContainer.AddChild(lShot);

        Effect.BumpScale(this, baseScale);
    }

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        if (!isCharging)
            velocity = new Vector2(angleX, angleY);
    }

    private void StartCharge(Timer pChargeTimer, Timer pAttackTimer)
    {
        if (!isCharging)
            isCharging = true;

        shellProjectileProtect = true;
        facePlayer = false;

        pAttackTimer.Stop();

        wing.SetSpeedDouble(true);

        Effect.SquashStretch(this, baseScale, false);

        Vector2 lDirection = playerDirection.Normalized();
        Vector2 lRecoil = Position - lDirection * recoilDistance;

        speed = chargeSpeed;
        velocity = lDirection;

        Tween lTween = CreateTween();

        lTween.Parallel().TweenProperty(this, (string)PropertyName.Position, lRecoil, recoilDuration)
             .SetTrans(Tween.TransitionType.Back)
             .SetEase(Tween.EaseType.Out);

        lTween.Parallel().TweenProperty(shell, (string)PropertyName.Position, basePosShell, recoilDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
    }

    public override void EntityDeath()
    {
        base.EntityDeath();
        shotTimer.Stop();
        chargeTimer.Stop();
    }
}
