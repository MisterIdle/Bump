using Godot;
using System;

public partial class Player : Entity
{
    private static Player instance;
    public static Player GetInstance()
    {
        return instance;
    }

    [ExportCategory("God Mode")]
    [Export] public bool godMode;
    [Export] public CollisionShape2D collision;

    [ExportCategory("Damage")]
    [Export] private float alphaNormal = 1f;
    [Export] private float alphaInvincible = 0.1f;

    [Export] private float alphaTweenTime = 0.1f;

    [Export] private float flashTime = 0.2f;

    private Tween invincibleTween;

    [ExportCategory("Movement")]
    [Export] private float dashMultiplier = 3f;
    private Vector2 dashDirection = Vector2.Right;
    [Export] private float backDash = 0.2f;

    [ExportCategory("Shot")]
    [ExportGroup("General")]
    [Export] private float baseShotSpace = 200f;
    [Export] private float shotSpaceLevelDivide = 1.5f;
    [Export] private PackedScene shotScene;
    [Export] private PackedScene fireBallScene;
    [Export] private Sprite2D nose;
    [Export] private Marker2D muzzle;

    [ExportGroup("Speed")]
    [Export] private float shotBaseSpeed = 600f;
    [Export] private float shotSpeedMultiplierLevelB = 1.1f;
    [Export] private float shotSpeedMultiplierLevelC = 1.3f;

    [ExportGroup("Scale")]
    [Export] private float shotScaleLevelB = 1.1f;
    [Export] private float shotScaleLevelC = 1.3f;

    [ExportCategory("Pow")]
    [Export] private int powNum = 1;
    [Export] private PackedScene powScene;

    [ExportCategory("Level")]
    private int level = 1;
    private int minLevel = 1;
    private int maxLevel = 3;

    [ExportCategory("Special Feature")]
    [Export] private Hammer hammer;

    [Export] private AnimatedSprite2D pall;

    private Shot lastShot;
    private bool isDashing;
    private float dashSpeed;
    private Timer dashTimer;

    private const int MARGIN = -20;

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else
        {
            QueueFree();
            return;
        }
    }

    public Player() : base() { }

    public override void Initiate()
    {
        base.Initiate();

        dashTimer = new Timer();

        dashTimer.OneShot = true;
        dashTimer.Timeout += EndDash;

        AddChild(dashTimer);

        pall.Play();

        hammer.Initiate();
        hammer.SetPlayer(this);

        UpdateLevelParameters();
    }

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        Move(pDelta);
        OutOfBound();

        hammer.Attack(pDelta);

        if (Input.IsActionJustPressed(Property.INPUT_LEVEL_UP))
            LevelUp(true);

        if (Input.IsActionJustPressed(Property.INPUT_LEVEL_DOWN))
            LevelUp(false);

        if (Input.IsActionJustPressed(Property.INPUT_GOD))
            ToggleGodMode();

        if (Input.IsActionJustPressed(Property.INPUT_SMARTBOMB))
            Bomb();

        if (CanFire())
            Fire();
    }

    private void Move(float pDelta)
    {
        if (isDashing)
            velocity = dashDirection * dashMultiplier;
        else if (velocity != Vector2.Zero)
            velocity = Input.GetVector(Property.INPUT_LEFT, Property.INPUT_RIGHT, Property.INPUT_UP, Property.INPUT_DOWN);
        else
            velocity.Y = angleY;
    }

    private void OutOfBound()
    {
        Vector2 lOut = Utils.OutOfBoundAxes(Position, MARGIN, MARGIN);

        if (lOut != Vector2.Zero)
        {
            Vector2 lDashDir = Vector2.Zero;

            if (lOut.X != 0) lDashDir.X = -lOut.X;
            if (lOut.Y != 0) lDashDir.Y = -lOut.Y;

            Dash(backDash, lDashDir);

            HitCheckInvincible();
        }
    }

    public void Dash(float pDuration, Vector2 pDirection, bool pOpposite = false)
    {
        if (isDashing) return;

        dashDirection = pOpposite ? (Position - pDirection).Normalized() : pDirection.Normalized();

        isDashing = true;
        dashSpeed = speed * dashMultiplier;
        dashTimer.Start(pDuration);
    }

    private void EndDash()
    {
        isDashing = false;
    }

    private bool CanFire()
    {
        if (!Input.IsActionPressed(Property.INPUT_SHOT) || hammer.isActif)
            return false;

        if (!IsInstanceValid(lastShot))
            return true;

        float lShotSpace = baseShotSpace / (level * shotSpaceLevelDivide);
        return lastShot.Position.X - Position.X > lShotSpace;
    }

    private void Bomb()
    {
        Pow lPow = Pow.Create(powScene);
        GameManager.GetInstance().gameContainer.AddChild(lPow);
    }

    private void Fire()
    {
        Shot lShot;

        switch (level)
        {
            case 1:
                lShot = Shot.Create(shotScene, muzzle.GlobalPosition, Vector2.Right, 0.1f);
                break;

            case 2:
                lShot = Shot.Create(shotScene, muzzle.GlobalPosition, Vector2.Right, 0.05f);
                break;

            case 3:
                lShot = Shot.Create(fireBallScene, muzzle.GlobalPosition, Vector2.Right);
                break;

            default:
                throw new InvalidOperationException();
        }

        Effect.BumpScale(this, baseScale);
        Effect.SquashStretch(this, baseScale, true, false, 0.8f, 1f);

        GameManager.GetInstance().gameContainer.AddChild(lShot);
        lastShot = lShot;
    }

    public void LevelUp(bool pUp)
    {
        level += pUp ? 1 : -1;
        level = Mathf.Clamp(level, minLevel, maxLevel);

        UpdateLevelParameters();

        Time.SlowMotion(0.1f, 0.1f);
        Effect.BumpScale(this, baseScale);
    }

    private void UpdateLevelParameters()
    {
        switch (level)
        {
            case 1:
                break;

            case 2:
                break;

            case 3:
                break;

            default:
                break;
        }
    }


    private void ToggleGodMode()
    {
        godMode = !godMode;
        collision.Disabled = godMode;
        GD.Print("GOD MODE: " + godMode);
    }

    public void AddHealth()
    {
        health++;
        GD.Print("HEALTH:" + health);
        Effect.FlashColor(sprite, Colors.Green);
        Effect.BumpScale(this, baseScale);
        Time.SlowMotion(0.1f, 0.1f);
    }

    public void AddPow()
    {
        powNum++;
        GD.Print("POW:" + powNum);
        Effect.BumpScale(this, baseScale);
        Time.SlowMotion(0.1f, 0.1f);
    }

    private void HitCheckInvincible()
    {
        if (!invincible)
        {
            EntityHit();
            hammer.ResetToIdle();
        }
    }

    public override void EntityHit(int pDamage = 1)
    {
        if (godMode) return;

        base.EntityHit(pDamage);

        LevelUp(false);
        StartInvincible(invincibleTime);
    }

    protected override void StartInvincible(float pDuration)
    {
        base.StartInvincible(pDuration);

        Effect.FlashColor(this, Colors.Red, flashTime);
        Blink();
    }

    protected override void EndInvincible()
    {
        invincibleTween.Kill();
        invincible = false;
    }

    private void Blink()
    {
        invincibleTween = CreateTween();
        invincibleTween.SetLoops(0);

        invincibleTween.TweenProperty(sprite, (string)PropertyName.Modulate + ":a", alphaInvincible, alphaTweenTime)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        invincibleTween.TweenProperty(sprite, (string)PropertyName.Modulate + ":a", alphaNormal, alphaTweenTime)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        HitCheckInvincible();
        Dash(backDash, pArea.GlobalPosition, true);

        velocity = Vector2.Zero;
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}
