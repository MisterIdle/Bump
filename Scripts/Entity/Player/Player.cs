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
    [Export] private float invincibleTime = 3f;

    [Export] private float alphaNormal = 1f;
    [Export] private float alphaInvincible = 0.1f;

    [Export] private float alphaTweenTime = 0.1f;

    [Export] private float redTweenTime = 0.2f;

    public bool invincible;
    private Timer invincibleTimer;
    private Tween invincibleTween;

    [ExportCategory("Movement")]
    [Export] private float dashMultiplier = 1.5f;
    private Vector2 dashDirection = Vector2.Right;
    private float backDash = 0.1f;

    [ExportCategory("Shot")]
    [Export] private float baseShotSpace = 200f;
    [Export] private float shotSpaceLevelDivide = 1.5f;

    [Export] private PackedScene shotScene;
    [Export] private PackedScene fireBallScene;
    [Export] private Marker2D muzzle;

    [ExportCategory("Pow")]
    [Export] private PackedScene powScene;

    [ExportCategory("Visual")]
    [Export] private Sprite2D sprite;
    [Export] private Helmet helmet;

    [ExportCategory("Weapon")]
    [Export] private Hammer hammer;

    private Shot lastShot;
    private bool isDashing;
    private float dashSpeed;
    private Timer dashTimer;
    private int level = 1;

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else
        {
            QueueFree();
            return;
        }

        invincibleTimer = new Timer();

        invincibleTimer.OneShot = true;
        invincibleTimer.Timeout += EndInvincible;

        AddChild(invincibleTimer);
    }

    public override void Initiate()
    {
        base.Initiate();

        hammer.Initiate();

        dashTimer = new Timer();
        dashTimer.OneShot = true;
        dashTimer.Timeout += EndDash;
        AddChild(dashTimer);
    }

    protected override void AliveUpdate(float delta)
    {
        base.AliveUpdate(delta);

        Move(delta);
        hammer.Attack(delta);

        // DEBUG
        if (Input.IsActionJustPressed("ui_up"))
            ChangeLevel(true);
        if (Input.IsActionJustPressed("ui_down"))
            ChangeLevel(false);

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
        if (!Input.IsActionPressed(Property.INPUT_SHOT))
            return false;

        if (!IsInstanceValid(lastShot))
            return true;

        float lCurrentShotSpace = baseShotSpace / (level * shotSpaceLevelDivide);
        return lastShot.Position.X - Position.X > lCurrentShotSpace;
    }

    private void FirePow()
    {
        Pow lPow = Pow.Create(powScene, muzzle.GlobalPosition, Vector2.Right);
        GameManager.GetInstance().gameContainer.AddChild(lPow);
    }

    private void Fire()
    {
        switch (level)
        {
            case 1:
            case 2:
                lastShot = Shot.Create(shotScene, muzzle.GlobalPosition, Vector2.Right);
                break;
            case 3:
                lastShot = Shot.Create(fireBallScene, muzzle.GlobalPosition, Vector2.Right);
                break;
        }

        GameManager.GetInstance().gameContainer.AddChild(lastShot);
        Bump();
    }

    private void ChangeLevel(bool pUp)
    {
        level += pUp ? 1 : -1;
        level = Math.Clamp(level, 1, 3);
        helmet.SetLevel(level);
    }

    private void ToggleGodMode()
    {
        godMode = !godMode;
        collision.Disabled = godMode;
        GD.Print("GOD MODE: " + godMode);
    }

    public override void EntityHit()
    {
        if (invincible || godMode) return;

        base.EntityHit();
        StartInvincible(invincibleTime);
    }

    private void StartInvincible(float pDuration)
    {
        invincible = true;

        Flash();
        Blink();

        invincibleTimer.Start(pDuration);
    }

    private void EndInvincible()
    {
        invincibleTween.Kill();
        invincible = false;
    }

    private void Flash()
    {
        invincibleTween = CreateTween();

        invincibleTween.TweenProperty(sprite, (string)PropertyName.Modulate, Colors.Red, redTweenTime)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        invincibleTween.TweenProperty(sprite, (string)PropertyName.Modulate, Colors.White, redTweenTime)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
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
}
