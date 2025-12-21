using Godot;

public partial class Rushroops : ShelledEnemy
{
    [Export] private PackedScene shotScene;

    [Export] private float fireRate = 1.5f;
    [Export] private float rotationSpeed = 5f;

    [Export] private float chargeWait = 7f;
    [Export] private float chargeSpeed = 300f;
    [Export] private float recoilDistance = -25f;
    [Export] private float recoilDuration = 1f;

    private Timer shotTimer;
    private Timer chargeTimer;

    private Vector2 basePos;
    private Vector2 toPlayer;

    private bool isCharging = false;

    public override void Initiate()
    {
        base.Initiate();
        velocity = Vector2.Left;

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
        Shot lShot = Shot.Create(shotScene, GlobalPosition, -toPlayer);
        GameManager.GetInstance().gameContainer.AddChild(lShot);

        Bump();
    }

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        toPlayer = Look.DirectionTo(Player.GetInstance(), this);

        bool lLeft = RotationDegrees > 90f || RotationDegrees < -90f;
        Scale = new Vector2(1, lLeft ? -1 : 1);

        if (!isCharging)
            velocity = new Vector2(angleX, angleY);
    }

    private void StartCharge(Timer pChargeTimer, Timer pAttackTimer)
    {
        if (!isCharging)
            isCharging = true;

        shellProjectileProtect = true;

        pAttackTimer.Stop();

        Bump();

        Vector2 lDirection = toPlayer.Normalized();
        Vector2 lRecoil = Position - lDirection * recoilDistance;

        speed = -chargeSpeed;
        velocity = lDirection;
        Rotation = toPlayer.Angle();

        Tween lTween = CreateTween();

        lTween.Parallel().TweenProperty(this, (string)PropertyName.Position, lRecoil, recoilDuration)
             .SetTrans(Tween.TransitionType.Back)
             .SetEase(Tween.EaseType.Out);

        lTween.Parallel().TweenProperty(shell, (string)PropertyName.Position, basePos, recoilDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
    }

    protected override void EntityDeath()
    {
        base.EntityDeath();
        shotTimer.Stop();
        chargeTimer.Stop();
    }
}
