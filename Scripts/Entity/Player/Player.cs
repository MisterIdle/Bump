using Godot;
using Com.IsartDigital.Shmup.NData;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;
using Com.IsartDigital.Shmup.NHUD;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NStats;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;

public partial class Player : Entity
{
    [ExportCategory("Components")]
    [Export] public CollisionShape2D collision;
    [Export] private Sprite2D nose;
    [Export] private Marker2D muzzle;
    [Export] private Hammer hammer;

    [ExportCategory("Progression")]
    [Export] private PlayerLevelData[] levels;

    [ExportCategory("Base Stats")]
    [Export] private int maxHealth = 3;
    [Export] private bool godMode;
    [Export] private float flashTime = 0.2f;

    [ExportCategory("Visuals")]
    [ExportGroup("Alpha")]
    [Export] private float alphaNormal = 1f;
    [Export] private float alphaInvincible = 0.1f;
    [Export] private float alphaTweenTime = 0.1f;
    [Export] private float rotationLerpSpeed = 10f;

    [ExportCategory("Spawn Animation")]
    [Export] private float dropDuration = 1.0f;
    [Export] private float entranceOvershoot = 20f;
    [Export] private float minSpawnOffset = 100f;

    [ExportGroup("Spawn Wave Effect")]
    [Export] private float spawnWaveAmplitude = 5f;
    [Export] private float spawnRotationAmplitude = 10f;
    [Export] private float spawnWaveDuration = 0.2f;

    [ExportCategory("Movement")]
    [Export] private float dashMultiplier = 3f;
    [Export] private float backDash = 0.2f;

    [ExportCategory("Combat")]
    [ExportGroup("PowerUps")]
    [Export] private int powNum = 1;
    [Export] private PackedScene powScene;

    [ExportGroup("Death")]
    [Export] private PackedScene popPlayerDeath;
    [Export] private float deathSlowMoScale = 0.5f;
    [Export] private float deathTimerDuration = 3f;

    [Export] private float minBetweenShots = 600f;

    public int level = 1;
    public enum State { Spawn, Normal, Dash, Knockback }
    public State currentState;

    private int minPow = 1;
    private int maxPow = 3;
    private int minLevel = 1;
    private int maxLevel => levels != null ? levels.Length : 1;

    private bool isOutSide;
    private float dashSpeed;
    private Vector2 dashDirection = Vector2.Right;
    private Vector2 finalSpawnPosition;

    private Tween invincibleTween;
    private Tween spawnTween;
    private Tween wobbleTween;

    private Shot lastShot;
    private Timer movementTimer;

    private const int DEATH_MARGIN = -150;
    private const int NOT_OUT_MARGIN = -35;

    private const float SHOT_SQUASH_X = 0.8f;
    private const float SHOT_SQUASH_Y = 1f;

    private static Player instance;

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

    private PlayerLevelData GetCurrentLevelData()
    {
        if (levels == null || levels.Length == 0)
            return null;

        int lIndex = Mathf.Clamp(level - 1, 0, levels.Length - 1);
        return levels[lIndex];
    }

    private PlayerShotData GetCurrentShotData()
    {
        PlayerLevelData lData = GetCurrentLevelData();
        if (lData == null) return null;
        return lData.shot;
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

        health = maxHealth;

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
        if (currentState == State.Spawn) return;

        base.AliveUpdate(pDelta);

        hammer.Attack(pDelta);
        Move(pDelta);
        UpdateRotation(pDelta);

        if (currentState != State.Spawn)
        {
            if (Input.IsActionJustPressed(Property.INPUT_SMARTBOMB))
                Pow();

            if (CanFire())
                Fire();
        }
    }

    public override void _Input(InputEvent pEvent)
    {
        if (Input.IsActionJustPressed(Property.INPUT_GOD))
            ToggleGodMode();

        if (Input.IsActionJustPressed(Property.INPUT_LEVEL_UP))
            LevelUp(true);

        if (Input.IsActionJustPressed(Property.INPUT_LEVEL_DOWN))
            LevelUp(false);

        if (Input.IsActionJustPressed(Property.INPUT_POW_UP))
            AddPow();

        if (Input.IsActionJustPressed(Property.INPUT_POW_DOWN))
        {
            powNum--;
            HUD.GetInstance().UpdatePow(powNum);
        }
        if (Input.IsActionJustPressed(Property.INPUT_HEALTH_UP))
            AddHealth();

        if (Input.IsActionJustPressed(Property.INPUT_HEALTH_DOWN))
            HitCheckInvincible();

        if (Input.IsActionJustPressed(Property.INPUT_WIN))
            MenuManager.GetInstance().SetMenu(MenuManager.MenuState.Victory);
    }

    private void AnimatedSpawn()
    {
        spawnTween?.Kill();
        spawnTween = CreateTween();

        Vector2 lOvershootPos = finalSpawnPosition + new Vector2(0, entranceOvershoot);

        spawnTween.TweenProperty(this, (string)PropertyName.Position, lOvershootPos, dropDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);

        spawnTween.TweenProperty(this, (string)PropertyName.Position, finalSpawnPosition, dropDuration)
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

        wobbleTween.TweenProperty(sprite, (string)PropertyName.Position + ":y", -spawnWaveAmplitude, spawnWaveDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        wobbleTween.Parallel().TweenProperty(sprite, (string)PropertyName.RotationDegrees, spawnRotationAmplitude, spawnWaveDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        wobbleTween.Chain().TweenProperty(sprite, (string)PropertyName.Position + ":y", spawnWaveAmplitude, spawnWaveDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        wobbleTween.Parallel().TweenProperty(sprite, (string)PropertyName.RotationDegrees, -spawnRotationAmplitude, spawnWaveDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    public void AnimatedDespawn(float pDuration)
    {
        if (isDeath) return;

        currentState = State.Spawn;

        wobbleTween?.Kill();
        movementTimer?.Stop();

        sprite.Position = Vector2.Zero;
        sprite.Rotation = 0;

        spawnTween?.Kill();
        spawnTween = CreateTween();

        spawnTween.TweenProperty(this, (string)PropertyName.Position + ":y", finalSpawnPosition.Y + entranceOvershoot, pDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);

        spawnTween.Chain().TweenProperty(this, (string)PropertyName.Position + ":y", -minSpawnOffset, pDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);
    }

    private void OnSpawnFinished()
    {
        wobbleTween?.Kill();
        Tween lResetTween = CreateTween();
        lResetTween.SetParallel(true);

        lResetTween.TweenProperty(sprite, (string)PropertyName.Position, Vector2.Zero, 0);
        lResetTween.TweenProperty(sprite, (string)PropertyName.Rotation, 0f, 0);

        currentState = State.Normal;
        EndInvincible();

        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.HUD);
        AudioManager.GetInstance().PlayMusic(AudioManager.MusicType.Level);
    }

    private void Move(float pDelta)
    {
        PlayerLevelData lData = GetCurrentLevelData();
        float lCurrentSpeedMultiplier = lData != null ? lData.moveSpeedMultiplier : 1f;

        if (currentState == State.Dash || currentState == State.Knockback)
            velocity = dashDirection * dashMultiplier;

        else if (velocity != Vector2.Zero)
            velocity = Input.GetVector(Property.INPUT_LEFT, Property.INPUT_RIGHT, Property.INPUT_UP, Property.INPUT_DOWN) * lCurrentSpeedMultiplier;

        else
            velocity.Y = angleY;
    }

    public void Dash(float pDuration, Vector2 pDirection, bool pOpposite = false)
    {
        if (currentState != State.Normal) return;

        dashDirection = pOpposite ? (Position - pDirection).Normalized() : pDirection.Normalized();

        currentState = State.Dash;
        dashSpeed = speed * dashMultiplier;

        movementTimer.Start(pDuration);
    }

    public void ApplyKnockback(float pDuration, Vector2 pDirection)
    {
        if (currentState == State.Spawn) return;

        dashDirection = pDirection.Normalized();
        currentState = State.Knockback;

        movementTimer.Start(pDuration);
    }

    private void OnMovementTimerTimeout()
    {
        if (currentState == State.Dash || currentState == State.Knockback)
            currentState = State.Normal;
    }

    private void Fire()
    {
        PlayerShotData lShotData = GetCurrentShotData();
        if (lShotData == null) return;

        AudioManager.GetInstance().Play(AudioManager.AudioType.PlayerShoot);
        Effect.BumpScale(sprite, baseSpriteScale);
        Effect.SquashStretch(sprite, baseSpriteScale, true, false, SHOT_SQUASH_X, SHOT_SQUASH_Y);

        Shot lShot = Shot.Create(lShotData.shotScene, muzzle.GlobalPosition, Vector2.Right);

        GameStats.shotsFired++;

        if (IsInstanceValid(lShot))
            lastShot = lShot;
    }

    private bool CanFire()
    {
        if (!Input.IsActionPressed(Property.INPUT_SHOT) || hammer.isActif)
            return false;

        PlayerShotData lShotData = GetCurrentShotData();
        if (lShotData == null)
            return false;

        if (!IsInstanceValid(lastShot) || !lastShot.IsInsideTree())
        {
            lastShot = null;
            return true;
        }

        float lDistance = lastShot.GlobalPosition.X - muzzle.GlobalPosition.X;
        return lDistance > minBetweenShots;
    }


    private void Pow()
    {
        if (powNum <= 0) return;

        Pow lPow = NPlayer.Pow.Create(powScene);
        GameManager.GetInstance().gameContainer.AddChild(lPow);

        if (!godMode)
            powNum--;

        HUD.GetInstance().UpdatePow(powNum);
    }

    private void OutOfBound()
    {
        if (currentState == State.Spawn) return;

        if (isDeath)
        {
            if (Utils.IsOutOfBound(Position, DEATH_MARGIN))
            {
                if (!isOutSide)
                {
                    isOutSide = true;
                    TriggerGameOverSequence();
                }
            }
            return;
        }

        Vector2 lClamped = Utils.ClampToScreen(Position, NOT_OUT_MARGIN);

        if (Position != lClamped)
        {
            Position = lClamped;

            if (currentState == State.Dash || currentState == State.Knockback)
            {
                velocity = Vector2.Zero;
                dashDirection = Vector2.Zero;
                currentState = State.Normal;
                movementTimer.Stop();
            }
        }
    }

    public void LevelUp(bool pUp)
    {
        level += pUp ? 1 : -1;
        level = Mathf.Clamp(level, minLevel, maxLevel);

        UpdateLevelParameters();
        ScaleTime.SlowMotion();
        Effect.BumpScale(this, baseScale);
    }

    private void UpdateLevelParameters()
    {
        PlayerLevelData lData = GetCurrentLevelData();
        sprite.SelfModulate = lData.playerColor;
    }

    public void AddHealth()
    {
        if (health >= maxHealth) return;

        AudioManager.GetInstance().Play(AudioManager.AudioType.PowerLife);

        health = Mathf.Min(health + 1, maxHealth);

        HUD.GetInstance().UpdateHealth(health);
        Effect.FlashColor(sprite, Colors.Green);
        Effect.BumpScale(this, baseScale);
        ScaleTime.SlowMotion();
    }

    public void AddPow()
    {
        if (powNum >= maxPow) return;
        AudioManager.GetInstance().Play(AudioManager.AudioType.PowerUpPow);

        powNum = Mathf.Min(powNum + 1, maxPow);

        HUD.GetInstance().UpdatePow(powNum);
        Effect.BumpScale(this, baseScale);
        ScaleTime.SlowMotion();
    }

    public void ToggleGodMode()
    {
        godMode = !godMode;
        collision.Disabled = godMode;

        Modulate = godMode ? Colors.Gold : Colors.White;
        HUD.GetInstance().SetGodMode(godMode);
    }

    private void Blink()
    {
        invincibleTween?.Kill();

        invincibleTween = CreateTween();
        invincibleTween.SetLoops();

        invincibleTween.TweenProperty(sprite, (string)PropertyName.Modulate + ":a", alphaInvincible, alphaTweenTime)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        invincibleTween.TweenProperty(sprite, (string)PropertyName.Modulate + ":a", alphaNormal, alphaTweenTime)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    private void UpdateRotation(float pDelta)
    {
        float lTargetAngle = 0f;

        PlayerLevelData lData = GetCurrentLevelData();
        float lCurrentRotSpeed = lData.rotationSpeed;

        if (velocity.X > 0)
            lTargetAngle = lCurrentRotSpeed;
        else if (velocity.X < 0)
            lTargetAngle = -lCurrentRotSpeed;

        sprite.RotationDegrees = Mathf.Lerp(sprite.RotationDegrees, lTargetAngle, rotationLerpSpeed * pDelta);
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

        if (invincibleTimer != null)
            invincibleTimer.Stop();
        health = 0;

        HUD.GetInstance().UpdateHealth(health);
        EntityDeath();
    }

    protected override void EndInvincible()
    {
        invincibleTween?.Kill();

        invincible = false;

        sprite.Modulate = new Color(sprite.Modulate.R, sprite.Modulate.G, sprite.Modulate.B, alphaNormal);
    }

    public override void EntityHit(int pDamage = 1)
    {
        if (godMode || currentState == State.Spawn) return;

        base.EntityHit(pDamage);

        if (health <= 0) SetDeadState();

        LevelUp(false);
        lastShot = null;

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

        Particule.Create(popPlayerDeath, Position);

        Timer lTimer = new Timer();
        lTimer.WaitTime = deathTimerDuration;
        lTimer.Timeout += AfterDeath;

        AddChild(lTimer);
        lTimer.Start();

        ScaleTime.SlowMotion(deathSlowMoScale, (int)deathTimerDuration);

        HUD.GetInstance().PlayGameOver();
        AudioManager.GetInstance().Play(AudioManager.AudioType.PlayerExplosion);
    }

    private void AfterDeath()
    {
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.GameOver);
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        HitCheckInvincible();

        Vector2 lDirection = Utils.DirectionTo(pArea.Position, Position);
        ApplyKnockback(backDash, lDirection);

        velocity = Vector2.Zero;
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}