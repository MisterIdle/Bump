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

    private Vector2 basePos;
    private Vector2 toPlayer;

    private bool isCharging = false;

    public override void Initiate()
    {
        base.Initiate();
        velocity = Vector2.Left;

        Timer lShotTimer = new Timer();
        AddChild(lShotTimer);

        lShotTimer.WaitTime = fireRate;
        lShotTimer.Autostart = true;
        lShotTimer.Timeout += Shoot;

        lShotTimer.Start();

        Timer lAttackTimer = new Timer();
        AddChild(lAttackTimer);

        lAttackTimer.WaitTime = chargeWait;
        lAttackTimer.Autostart = true;
        lAttackTimer.Timeout += () => StartCharge(lAttackTimer, lShotTimer);
        
        lAttackTimer.Start();

        shell.SetShellColor(Colors.Blue);
    }

    private void Shoot()
    {
        Shot lShot = Shot.Create(shotScene, GlobalPosition, -toPlayer);
        GameManager.GetInstance().gameContainer.AddChild(lShot);

        Bump();
    }

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

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

        shellProtect = true;

        pAttackTimer.Stop();

        Bump();

        Vector2 lDirection = toPlayer.Normalized();
        Vector2 lRecoil = Position - lDirection * recoilDistance;

        speed = -chargeSpeed;
        velocity = lDirection;
        Rotation = toPlayer.Angle();

        Tween lTween = CreateTween();

        lTween.Parallel().TweenProperty(this, Property.TWEEN_POSITION, lRecoil, recoilDuration)
             .SetTrans(Tween.TransitionType.Back)
             .SetEase(Tween.EaseType.Out);

        lTween.Parallel().TweenProperty(shell, Property.TWEEN_POSITION, basePos, recoilDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        lTween.Parallel().TweenProperty(shell.sprite, Property.TWEEN_SELF_MODULATE, Colors.DarkBlue, recoilDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
    }
}
