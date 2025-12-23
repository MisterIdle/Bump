using Godot;
using System;
using System.Reflection.Metadata;

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
    [Export] private float invincibleTime = 3f;

    [Export] private float alphaNormal = 1f;
    [Export] private float alphaInvincible = 0.1f;

    [Export] private float alphaTweenTime = 0.1f;

    [Export] private float flashTime = 0.2f;

    public bool invincible;
    private Timer invincibleTimer;
    private Tween invincibleTween;

    [ExportCategory("Movement")]
    [Export] private float dashMultiplier = 1.5f;
    private Vector2 dashDirection = Vector2.Right;
    private float backDash = 0.1f;

    [ExportCategory("Shot")]
    [ExportGroup("General")]
    [Export] private float baseShotSpace = 200f;
    [Export] private float shotSpaceLevelDivide = 1.5f;
    [Export] private PackedScene shotScene;
    [Export] private PackedScene fireBallScene;
    [Export] private Marker2D muzzle;

    [ExportGroup("Speed")]
    [Export] private float shotBaseSpeed = 600f;
    [Export] private float shotSpeedMultiplierLevelB = 1.1f;
    [Export] private float shotSpeedMultiplierLevelC = 1.3f;

    [ExportGroup("Scale")]
    [Export] private float shotScaleLevelB = 1.1f;
    [Export] private float shotScaleLevelC = 1.3f;

    [ExportGroup("Color")]
    [Export] private Color shotColorLevelB = Colors.Yellow;
    [Export] private Color shotColorLevelC = Colors.Orange;

    [ExportCategory("Pow")]
    [Export] private int powNum = 1;
    [Export] private PackedScene powScene;

    [ExportCategory("Level")]
    private int level = 1;
    private int minLevel = 1;
    private int maxLevel = 3;

    [ExportCategory("Visual")]
    [Export] private Sprite2D sprite;
    [Export] private Helmet helmet;

    [ExportCategory("Weapon")]
    [Export] private Hammer hammer;

    private Shot lastShot;
    private bool isDashing;
    private float dashSpeed;
    private Timer dashTimer;

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

        hammer.Initiate();

        dashTimer = new Timer();
        dashTimer.OneShot = true;
        dashTimer.Timeout += EndDash;

        AddChild(dashTimer);

        invincibleTimer = new Timer();

        invincibleTimer.OneShot = true;
        invincibleTimer.Timeout += EndInvincible;

        AddChild(invincibleTimer);
    }

    protected override void AliveUpdate(float delta)
    {
        base.AliveUpdate(delta);

        Move(delta);
        hammer.Attack(delta);

        if (Input.IsActionJustPressed(Property.INPUT_GOD))
            ToggleGodMode();

        if (Input.IsActionJustPressed(Property.INPUT_SMARTBOMB))
            FirePow();

        if (CanFire())
            Fire();
    }

    private void Move(float pDelta)
    {
        if (isDashing)
            velocity = dashDirection * dashSpeed * pDelta;
        else if (velocity != Vector2.Zero)
            velocity = Input.GetVector(Property.INPUT_LEFT, Property.INPUT_RIGHT, Property.INPUT_UP, Property.INPUT_DOWN);
        else
            velocity.Y = angleY;
    }

    public void Dash(float pDuration, Vector2 pDirection, bool Opposite = false)
    {
        if (isDashing) return;

        dashDirection = Opposite ? (Position - pDirection).Normalized() : pDirection.Normalized();

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
        return Math.Abs(lastShot.Position.X - Position.X) > lShotSpace;
    }

    private void FirePow()
    {
        Pow lPow = Pow.Create(powScene, muzzle.GlobalPosition, Vector2.Right);
        GameManager.GetInstance().gameContainer.AddChild(lPow);
    }

    private void Fire()
    {
        Shot lShot;

        switch (level)
        {
            case 1:
                lShot = Shot.Create(shotScene, muzzle.GlobalPosition, Vector2.Right, 0.1f);
                lShot.SetSpeed(shotBaseSpeed);
                break;

            case 2:
                lShot = Shot.Create(shotScene, muzzle.GlobalPosition, Vector2.Right, 0.05f);
                lShot.SetVisualParameters(shotColorLevelB, shotScaleLevelB);
                lShot.SetSpeed(shotBaseSpeed * shotSpeedMultiplierLevelB);
                break;

            case 3:
                lShot = Shot.Create(fireBallScene, muzzle.GlobalPosition, Vector2.Right);
                lShot.SetVisualParameters(shotColorLevelC, shotScaleLevelC);
                lShot.SetSpeed(shotBaseSpeed * shotSpeedMultiplierLevelC);
                break;

            default:
                throw new InvalidOperationException();
        }

        Effect.BumpScale(this);

        GameManager.GetInstance().gameContainer.AddChild(lShot);
        lastShot = lShot;
    }

    public void LevelUp(bool pUp)
    {
        level += pUp ? 1 : -1;
        level = Mathf.Clamp(level, minLevel, maxLevel);

        if (level == maxLevel)
            AddHealth();

        helmet.SetLevel(level);

        Effect.BumpScale(this);
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
        Effect.FlashEntityColor(sprite, Colors.Green);
        Effect.BumpScale(this);
    }

    public void AddPow()
    {
        powNum++;
        GD.Print("POW:" + powNum);
        Effect.BumpScale(this);
    }

    public override void EntityHit()
    {
        if (invincible || godMode) return;

        base.EntityHit();

        LevelUp(false);
        StartInvincible(invincibleTime);
    }

    private void StartInvincible(float pDuration)
    {
        invincible = true;

        Effect.FlashEntityColor(this, Colors.Red, flashTime);
        Blink();

        invincibleTimer.Start(pDuration);
    }

    private void EndInvincible()
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

        velocity = Vector2.Zero;

        if (invincible)
        {
            Dash(backDash, pArea.GlobalPosition, true);
        }
        else
        {
            Dash(backDash, pArea.GlobalPosition, true);
            EntityHit();

            hammer.ResetToIdle();
        }
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}
