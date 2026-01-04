using Godot;
using System;

public partial class Player : Entity
{
    public enum State
    {
        Spawn,
        Normal,
        Dash,
        Knockback
    }

    private static Player instance;

    [ExportCategory("Components")]
    [Export] public CollisionShape2D collision;
    [Export] private Sprite2D nose;
    [Export] private Marker2D muzzle;
    [Export] private Hammer hammer;

    [ExportCategory("Stats")]
    [Export] private int maxHealth = 5;
    [Export] private bool godMode;
    [Export] private float flashTime = 0.2f;

    [ExportCategory("Visuals")]
    [ExportGroup("Alpha")]
    [Export] private float alphaNormal = 1f;
    [Export] private float alphaInvincible = 0.1f;
    [Export] private float alphaTweenTime = 0.1f;

    [ExportCategory("Spawn Animation")]
    [Export] private float dropDuration = 1.0f;
    [Export] private float entranceOvershoot = 20f;
    [Export] private float minSpawnOffset = 100f;

    [ExportGroup("Spawn Wave Effect")]
    [Export] private float spawnWaveAmplitude = 5f;
    [Export] private float spawnRotationAmplitude = 10f;
    [Export] private float spawnWaveDuration = 0.2f;

    [ExportGroup("Colors")]
    [Export] private Color levelAColor;
    [Export] private Color levelBColor;
    [Export] private Color levelCColor;

    [ExportCategory("Movement")]
    [Export] private float dashMultiplier = 3f;
    [Export] private float backDash = 0.2f;

    [ExportCategory("Combat")]
    [ExportGroup("Shooting")]
    [Export] private PackedScene shotScene;
    [Export] private PackedScene fireBallScene;
    [Export] private float baseShotSpace = 200f;
    [Export] private float shotSpaceLevelDivide = 1.5f;
    [Export] private float shotBaseSpeed = 600f;
    [Export] private float shotSpeedMultiplierLevelB = 1.1f;
    [Export] private float shotSpeedMultiplierLevelC = 1.3f;
    [Export] private float shotScaleLevelB = 1.1f;
    [Export] private float shotScaleLevelC = 1.3f;

    [ExportGroup("PowerUps")]
    [Export] private int powNum = 1;
    [Export] private PackedScene powScene;

    [ExportGroup("Death")]
    [Export] private PackedScene popPlayerDeath;
    [Export] private float deathSlowMoScale = 0.5f;
    [Export] private float deathTimerDuration = 3f;

    public int level = 1;
    public State currentState;

    private int minPow = 1;
    private int maxPow = 3;
    private int minLevel = 1;
    private int maxLevel = 3;

    private bool isOutSide;
    private float dashSpeed;
    private Vector2 dashDirection = Vector2.Right;
    private Vector2 finalSpawnPosition;

    private Tween invincibleTween;
    private Tween spawnTween;
    private Tween wobbleTween;
    private Shot lastShot;
    private Timer movementTimer;

    private const int MARGIN = 50;

    private Vector2 targetPosition;

    public static Player GetInstance()
    {
        return instance;
    }

    public Player() : base() { }

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else
        {
            QueueFree();
            return;
        }
    }

    public override void Initiate()
    {
        base.Initiate();

        GameStats.Reset();

        currentState = State.Spawn;

        finalSpawnPosition = Position;
        Position = new Vector2(finalSpawnPosition.X, -minSpawnOffset);

        movementTimer = new Timer();
        movementTimer.OneShot = true;
        movementTimer.Timeout += OnMovementTimerTimeout;
        AddChild(movementTimer);

        hammer.Initiate();
        hammer.SetPlayer(this);

        UpdateLevelParameters();

        AnimatedSpawn();

        HUD.GetInstance().UpdateHealth(health);
        HUD.GetInstance().UpdatePow(powNum);
    }


    protected override void Update(float pDelta)
    {
        base.Update(pDelta);
        OutOfBound();
    }

    protected override void AliveUpdate(float pDelta)
    {
        if (currentState == State.Spawn)
            return;

        base.AliveUpdate(pDelta);
        Move(pDelta);
        hammer.Attack(pDelta);
        HandleDebugInput();
        HandleGameInput();
    }

    private void AnimatedSpawn()
    {
        if (spawnTween != null) spawnTween.Kill();
        spawnTween = CreateTween();

        Vector2 lOvershootPos = finalSpawnPosition + new Vector2(0, entranceOvershoot);

        spawnTween.TweenProperty(this, "position", lOvershootPos, dropDuration * 0.7f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);

        spawnTween.TweenProperty(this, "position", finalSpawnPosition, dropDuration * 0.3f)
            .SetTrans(Tween.TransitionType.Bounce)
            .SetEase(Tween.EaseType.In);

        spawnTween.Finished += OnSpawnFinished;

        StartSpawnWobble();
    }

    private void StartSpawnWobble()
    {
        sprite.Position = Vector2.Zero;
        sprite.Rotation = 0;

        wobbleTween = CreateTween();
        wobbleTween.SetLoops();

        wobbleTween.TweenProperty(sprite, "position:y", -spawnWaveAmplitude, spawnWaveDuration * 0.5f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        wobbleTween.Parallel().TweenProperty(sprite, "rotation_degrees", spawnRotationAmplitude, spawnWaveDuration * 0.5f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        wobbleTween.Chain().TweenProperty(sprite, "position:y", spawnWaveAmplitude, spawnWaveDuration * 0.5f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        wobbleTween.Parallel().TweenProperty(sprite, "rotation_degrees", -spawnRotationAmplitude, spawnWaveDuration * 0.5f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    public void AnimatedDespawn(float pDuration)
    {
        if (isDeath)
            return;

        currentState = State.Spawn;

        wobbleTween?.Kill();
        movementTimer?.Stop();

        sprite.Position = Vector2.Zero;
        sprite.Rotation = 0;

        if (spawnTween != null) spawnTween.Kill();
        spawnTween = CreateTween();

        spawnTween.TweenProperty(this, "position:y", finalSpawnPosition.Y + entranceOvershoot, pDuration * 0.3f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);

        spawnTween.Chain().TweenProperty(this, "position:y", -minSpawnOffset, pDuration * 0.7f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);
    }

    private void OnSpawnFinished()
    {
        wobbleTween?.Kill();

        Tween resetTween = CreateTween();
        resetTween.SetParallel(true);
        resetTween.TweenProperty(sprite, "position", Vector2.Zero, 0.1f);
        resetTween.TweenProperty(sprite, "rotation", 0f, 0.1f);

        currentState = State.Normal;
        EndInvincible();

        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.HUD);
        AudioManager.GetInstance().PlayMusic(AudioManager.MusicType.Level);
    }

    public override void EntityHit(int pDamage = 1)
    {
        if (godMode || currentState == State.Spawn)
            return;

        base.EntityHit(pDamage);

        if (health <= 0)
        {
            EntityDeath();
            SetDeadState();
        }

        LevelUp(false);
        HUD.GetInstance().UpdateHealth(health);
        StartInvincible(invincibleTime);
    }

    private void SetDeadState()
    {
        invincibleTween?.Kill();
        spawnTween?.Kill();

        sprite.Modulate = Colors.White;
        movementTimer.Stop();
        velocity = Vector2.Zero;
        AudioManager.GetInstance().Play(AudioManager.AudioType.PlayerExplosion);
    }

    private void TriggerGameOverSequence()
    {
        isOutSide = true;
        Particule.Create(popPlayerDeath, GlobalPosition);

        Timer lTimer = new Timer();
        lTimer.WaitTime = deathTimerDuration;
        Time.SlowMotion(deathSlowMoScale, (int)deathTimerDuration);

        AddChild(lTimer);
        lTimer.Timeout += AfterDeath;
        lTimer.Start();

        HUD.GetInstance().PlayGameOver();
        AudioManager.GetInstance().Play(AudioManager.AudioType.PlayerExplosion);
    }

    private void AfterDeath()
    {
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.GameOver);
    }

    private void OutOfBound()
    {
        if (currentState == State.Spawn)
            return;

        Vector2 lOut = Utils.OutOfBoundAxes(Position, MARGIN, MARGIN);

        if (lOut == Vector2.Zero)
            return;

        if (isDeath)
        {
            if (!isOutSide && lOut.Y > 0)
            {
                isOutSide = true;
                TriggerGameOverSequence();
            }
            return;
        }

        Vector2 lKnockbackDir = Vector2.Zero;

        if (lOut.X != 0)
            lKnockbackDir.X = -lOut.X;

        if (lOut.Y != 0)
            lKnockbackDir.Y = -lOut.Y;

        ApplyKnockback(backDash, lKnockbackDir);
        HitCheckInvincible();
    }

    bool isFlipped = false;

    private void Move(float pDelta)
    {
        if (currentState == State.Dash || currentState == State.Knockback)
            velocity = dashDirection * dashMultiplier;

        else if (velocity != Vector2.Zero)
            velocity = Input.GetVector(Property.INPUT_LEFT, Property.INPUT_RIGHT, Property.INPUT_UP, Property.INPUT_DOWN);

        else
            velocity.Y = angleY;
    }

    public void Dash(float pDuration, Vector2 pDirection, bool pOpposite = false)
    {
        if (currentState != State.Normal)
        {
            return;
        }

        dashDirection = pOpposite ? (Position - pDirection).Normalized() : pDirection.Normalized();
        currentState = State.Dash;
        dashSpeed = speed * dashMultiplier;
        movementTimer.Start(pDuration);
    }

    public void ApplyKnockback(float pDuration, Vector2 pDirection)
    {
        if (currentState == State.Spawn)
            return;

        dashDirection = pDirection.Normalized();
        currentState = State.Knockback;
        movementTimer.Start(pDuration);
    }

    private void OnMovementTimerTimeout()
    {
        if (currentState == State.Dash || currentState == State.Knockback)
            currentState = State.Normal;
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        HitCheckInvincible();

        Vector2 lDirection = (Position - pArea.GlobalPosition).Normalized();
        ApplyKnockback(backDash, lDirection);

        velocity = Vector2.Zero;
    }

    private void HandleGameInput()
    {
        if (currentState == State.Knockback)
            return;

        if (Input.IsActionJustPressed(Property.INPUT_GOD))
            ToggleGodMode();

        if (Input.IsActionJustPressed(Property.INPUT_SMARTBOMB))
            Bomb();

        if (CanFire())
            Fire();
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

        AudioManager.GetInstance().Play(AudioManager.AudioType.PlayerShoot);

        Effect.BumpScale(sprite, baseSpriteScale);
        Effect.SquashStretch(sprite, baseSpriteScale, true, false, 0.8f, 1f);

        GameManager.GetInstance().gameContainer.AddChild(lShot);
        lastShot = lShot;
    }

    private void Bomb()
    {
        if (powNum <= 0)
        {
            return;
        }

        Pow lPow = Pow.Create(powScene);
        GameManager.GetInstance().gameContainer.AddChild(lPow);

        if (!godMode)
            powNum--;

        HUD.GetInstance().UpdatePow(powNum);
    }

    public void LevelUp(bool pUp)
    {
        level += pUp ? 1 : -1;
        level = Mathf.Clamp(level, minLevel, maxLevel);

        UpdateLevelParameters();
        Time.SlowMotion();
        Effect.BumpScale(this, baseScale);
    }

    private void UpdateLevelParameters()
    {
        switch (level)
        {
            case 1:
                sprite.SelfModulate = levelAColor;
                break;
            case 2:
                sprite.SelfModulate = levelBColor;
                break;
            case 3:
                sprite.SelfModulate = levelCColor;
                break;
        }
    }

    public void AddHealth()
    {
        if (health >= maxHealth)
            return;

        AudioManager.GetInstance().Play(AudioManager.AudioType.PowerLife);
        health = Mathf.Min(health + 1, maxHealth);
        HUD.GetInstance().UpdateHealth(health);
        GD.Print("HEALTH:" + health);
        Effect.FlashColor(sprite, Colors.Green);
        Effect.BumpScale(this, baseScale);
        Time.SlowMotion();
    }

    public void AddPow()
    {
        if (powNum >= maxPow)
            return;

        AudioManager.GetInstance().Play(AudioManager.AudioType.PowerUpPow);
        powNum = Mathf.Min(powNum + 1, maxPow);
        HUD.GetInstance().UpdatePow(powNum);
        GD.Print("POW:" + powNum);
        Effect.BumpScale(this, baseScale);
        Time.SlowMotion();
    }

    private void ToggleGodMode()
    {
        godMode = !godMode;
        collision.Disabled = godMode;
        GD.Print("GOD MODE: " + godMode);

        Modulate = godMode ? Colors.Gold : Colors.White;

        HUD.GetInstance().SetGodMode(godMode);
    }

    private void HitCheckInvincible()
    {
        if (!invincible)
        {
            EntityHit();
            hammer.ResetToIdle();
            AudioManager.GetInstance().Play(AudioManager.AudioType.PlayerLostLife);
        }
    }

    protected override void StartInvincible(float pDuration)
    {
        base.StartInvincible(pDuration);
        Effect.FlashColor(this, Colors.Red, flashTime);
        Blink();
    }

    public void InstantDeath()
    {
        if (isDeath) return;

        godMode = false;
        invincible = false;
        if (invincibleTimer != null) invincibleTimer.Stop();

        health = 0;
        HUD.GetInstance().UpdateHealth(0);

        EntityDeath();
    }


    protected override void EndInvincible()
    {
        if (invincibleTween != null)
        {
            invincibleTween.Kill();
        }

        invincible = false;
        sprite.Modulate = new Color(sprite.Modulate.R, sprite.Modulate.G, sprite.Modulate.B, alphaNormal);
    }

    private void Blink()
    {
        if (invincibleTween != null)
        {
            invincibleTween.Kill();
        }

        invincibleTween = CreateTween();
        invincibleTween.SetLoops();

        invincibleTween.TweenProperty(sprite, "modulate:a", alphaInvincible, alphaTweenTime)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        invincibleTween.TweenProperty(sprite, "modulate:a", alphaNormal, alphaTweenTime)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    private void HandleDebugInput()
    {
        if (Input.IsActionJustPressed(Property.INPUT_LEVEL_UP))
        {
            LevelUp(true);
        }

        if (Input.IsActionJustPressed(Property.INPUT_LEVEL_DOWN))
        {
            LevelUp(false);
        }

        if (Input.IsActionJustPressed(Property.INPUT_POW_UP))
        {
            AddPow();
        }

        if (Input.IsActionJustPressed(Property.INPUT_POW_DOWN))
        {
            powNum--;
            HUD.GetInstance().UpdatePow(powNum);
        }

        if (Input.IsActionJustPressed(Property.INPUT_HEALTH_UP))
        {
            AddHealth();
        }

        if (Input.IsActionJustPressed(Property.INPUT_HEALTH_DOWN))
        {
            HitCheckInvincible();
        }

        if (Input.IsActionJustPressed(Property.INPUT_WIN))
        {
            MenuManager.GetInstance().SetMenu(MenuManager.MenuState.Victory);
        }
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}