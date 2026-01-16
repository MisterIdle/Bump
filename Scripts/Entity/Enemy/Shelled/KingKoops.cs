using Godot;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;
using Com.IsartDigital.Shmup.NHUD;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NMusic;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class KingKoops : ShelledEnemy
{
    [ExportCategory("References")]
    [Export] private Area2D head;
    [Export] private Sprite2D spriteHead;
    [Export] private TickEvent musicSync;
    [Export] private PackedScene koopsScene;

    [ExportCategory("Phase Logic")]
    [ExportGroup("Final Phase")]
    [Export] private PackedScene specialSpikeScene;
    [Export] private Color phaseFinalColor = Colors.Purple;

    [ExportCategory("Combat Stats")]
    [Export] private float introDuration = 3.0f;
    [Export] private float transitionDuration = 0.4f;
    [Export] private float deathDelayDuration = 3.0f;

    [ExportCategory("Dash Behavior")]
    [Export] private float preDashDuration = 0.5f;
    [Export] private float dashDuration = 0.5f;
    [Export] private float recoveryDuration = 1.0f;
    [Export] private float dashSpeed = 800f;
    [Export] private float recoilDistance = 50f;
    [Export] private float deceleration = 5f;

    [ExportCategory("Knockback Logic")]
    [Export] private float hammerKnockbackForce = 600f;
    [Export] private float knockbackDecay = 8f;

    [Export] private float timerSpawn = 2.5f;

    [ExportCategory("Idle & Visuals")]
    [Export] private float grooveJumpHeight = 25f;
    [Export] private float grooveTiltAngle = 8f;
    [Export] private float grooveStepDuration = 0.15f;
    [Export] private float grooveReturnDuration = 0.2f;

    [ExportCategory("Transform Targets")]
    [ExportSubgroup("Head")]
    [Export] private Vector2 headRetractedPos = Vector2.Zero;
    [Export] private Vector2 headRetractedScale = Vector2.One;
    [Export] private Vector2 headOutsidePos = new Vector2(15f, 0f);
    [Export] private Vector2 headOutsideScale = Vector2.One;

    [ExportSubgroup("Shell")]
    [Export] private Vector2 shellRetractedPos = Vector2.Zero;
    [Export] private float shellRetractedRot = 0f;
    [Export] private Vector2 shellOutsidePos = new Vector2(0f, -20f);
    [Export] private float shellOutsideRot = -25f;

    public enum EnemyState { Intro, Outside, Protected }
    private enum ActionState { Idle, PreDash, Dashing, Recovery }
    private enum PhaseState { Intro, Normal, Aggressive, Summoning, Final }

    private EnemyState currentState;
    private ActionState currentAction = ActionState.Idle;
    private PhaseState currentPhase = PhaseState.Intro;

    private Timer dashTimer;
    private Vector2 dashDirection;
    private Vector2 currentKnockback = Vector2.Zero;
    private int beatCounter = 0;

    private Tween visualTween;
    private Timer introTimer;
    private Timer spawnTimer;

    private ulong lastPhaseChangeTime = 0;
    private const ulong PHASE_COOLDOWN_MS = 2000;

    private const float SCREEN_CLAMP_LIMIT = 150f;
    private const int OUT_OF_BOUND_LIMIT = -150;
    private const float VELOCITY_MOVE_X = 1f;

    private const float TWEEN_OUTSIDE_RATIO = 0.6f;
    private const float TWEEN_HEAD_DELAY = 0.1f;
    private const float TWEEN_RETRACT_RATIO = 0.5f;
    private const float TWEEN_SHELL_DELAY = 0.05f;
    private const float TWEEN_SHELL_POP_TIME = 0.05f;
    private const float TWEEN_SHELL_RETURN_TIME = 0.1f;
    private const float SCALE_POP_FACTOR = 1.1f;

    private const int BEAT_MODULO = 2;
    private const float GROOVE_DIR_POSITIVE = 1f;
    private const float GROOVE_DIR_NEGATIVE = -1f;

    [Export] private int scoreHead = 5000;

    private bool victoryTriggered = false;

    public override void Initiate()
    {
        base.Initiate();

        SwitchState(EnemyState.Protected);

        musicSync.BeatDetected += OnMusicBeat;
        head.AreaEntered += OnHeadCollide;

        dashTimer = new Timer();
        dashTimer.OneShot = true;
        AddChild(dashTimer);

        spawnTimer = new Timer();
        spawnTimer.Timeout += SpawnKoops;
        spawnTimer.WaitTime = timerSpawn;
        AddChild(spawnTimer);

        StartIntro();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (isDeath && !victoryTriggered)
        {
            if (!AudioManager.GetInstance().IsMusicPlaying())
            {
                CallDeferred(MethodName.CallDeathCleanup);
            }
        }

        if (isDeath)
        {
            float lBottomLimit = GetViewportRect().Size.Y + 200;

            if (GlobalPosition.Y > lBottomLimit)
            {
                QueueFree();
            }
        }
    }

    protected override void AliveUpdate(float pDelta)
    {
        spriteHead.RotationDegrees = Utils.LookAt(playerPos, Position);

        if (currentAction != ActionState.Idle)
            UpdateDashLogic(pDelta);
        else
            PhaseAction(pDelta);

        base.AliveUpdate(pDelta);

        Vector2 lCenter = Camera.GetInstance().GetScreenCenter();

        if (Utils.IsOutOfBound(Position, OUT_OF_BOUND_LIMIT, true))
        {
            if (Position.X < lCenter.X)
            {
                direction = MoveDirection.Right;
                velocity.X = VELOCITY_MOVE_X;
            }
            else if (Position.X > lCenter.X)
            {
                direction = MoveDirection.Left;
                velocity.X = -VELOCITY_MOVE_X;
            }
        }

        if (currentPhase != PhaseState.Intro)
            Position = Utils.ClampToScreen(Position, SCREEN_CLAMP_LIMIT, SCREEN_CLAMP_LIMIT, SCREEN_CLAMP_LIMIT);
    }

    private void StartDashSequence()
    {
        if (currentAction != ActionState.Idle) return;

        currentKnockback = Vector2.Zero;
        currentAction = ActionState.PreDash;
        direction = MoveDirection.None;

        SwitchState(EnemyState.Protected);

        velocity = Vector2.Zero;

        PerformChargeAnimation();

        dashTimer.WaitTime = preDashDuration;
        dashTimer.Timeout += ExecuteDash;
        dashTimer.Start();
    }

    private void PerformChargeAnimation()
    {
        Vector2 lRecoilPos = GlobalPosition - (playerDirection.Normalized() * recoilDistance);

        KillActiveTweens();
        visualTween = CreateTween();

        visualTween.SetParallel(true);

        visualTween.TweenProperty(this, (string)PropertyName.GlobalPosition, lRecoilPos, preDashDuration)
                 .SetTrans(Tween.TransitionType.Back)
                 .SetEase(Tween.EaseType.Out);

        Effect.SquashStretch(this, baseScale, false, true);

        float lShakeAngle = 4f;
        float lShakeStep = preDashDuration / 4f;

        visualTween.TweenProperty(shell, (string)PropertyName.RotationDegrees, shell.RotationDegrees + lShakeAngle, lShakeStep);
        visualTween.TweenProperty(shell, (string)PropertyName.RotationDegrees, shell.RotationDegrees - lShakeAngle, lShakeStep).SetDelay(lShakeStep);
        visualTween.TweenProperty(shell, (string)PropertyName.RotationDegrees, shell.RotationDegrees + lShakeAngle, lShakeStep).SetDelay(lShakeStep * 2);
        visualTween.TweenProperty(shell, (string)PropertyName.RotationDegrees, shell.RotationDegrees, lShakeStep).SetDelay(lShakeStep * 3);

        shell.SetShellColor(Colors.OrangeRed);

        visualTween.TweenProperty(spriteHead, "self_modulate", Colors.Red, preDashDuration)
            .From(Colors.White);
    }

    private void ExecuteDash()
    {
        dashTimer.Timeout -= ExecuteDash;

        currentAction = ActionState.Dashing;

        if (currentPhase != PhaseState.Summoning)
            dashDirection = playerDirection.Normalized();

        SwitchState(EnemyState.Outside);

        Effect.SquashStretch(this, baseScale, false, false);

        if (visualTween != null && visualTween.IsValid())
        {
            shell.SetShellColor(Colors.Orange);

            visualTween.SetParallel(true);
        }

        dashTimer.WaitTime = dashDuration;
        dashTimer.Timeout += EndDash;
        dashTimer.Start();
    }

    private void EndDash()
    {
        dashTimer.Timeout -= EndDash;

        currentAction = ActionState.Recovery;

        dashTimer.WaitTime = recoveryDuration;
        dashTimer.Timeout += Recovered;
        dashTimer.Start();

        shell.SetShellColor(Colors.Black);
    }

    private void Recovered()
    {
        SwitchState(EnemyState.Protected);
        dashTimer.Timeout -= Recovered;
        currentAction = ActionState.Idle;
    }

    private void UpdateDashLogic(float pDelta)
    {
        currentKnockback = currentKnockback.Lerp(Vector2.Zero, knockbackDecay * pDelta);

        switch (currentAction)
        {
            case ActionState.PreDash:
                velocity = Vector2.Zero;
                break;

            case ActionState.Dashing:
                if (currentPhase == PhaseState.Summoning)
                    dashDirection = playerDirection;

                velocity = (dashDirection * dashSpeed) + currentKnockback;
                break;

            case ActionState.Recovery:
                velocity = velocity.Lerp(Vector2.Zero, deceleration * pDelta);
                break;
        }
    }

    private void PhaseAction(float pDelta)
    {
        switch (currentPhase)
        {
            case PhaseState.Normal:
                velocity.Y = angleY;
                break;

            case PhaseState.Aggressive:
                velocity = playerDirection;
                break;

            case PhaseState.Summoning:
                velocity.Y = angleY;
                break;

            case PhaseState.Final:
                velocity = Vector2.Zero;
                break;
        }
    }

    private void OnMusicBeat(TickEvent.BeatTypeBoss pType)
    {
        if (isDeath) return;

        switch (pType)
        {
            case TickEvent.BeatTypeBoss.Phase:
                AdvancePhase();
                break;

            case TickEvent.BeatTypeBoss.Shoot:
                TryFireSpike();
                break;

            case TickEvent.BeatTypeBoss.Reload:
                SpawnSpikes();
                break;

            case TickEvent.BeatTypeBoss.Dash:
                StartDashSequence();
                break;

            case TickEvent.BeatTypeBoss.Move:
                DoGrooveAnimation();
                break;
        }
    }

    private void StartIntro()
    {
        AudioManager.GetInstance().StopMusic();
        currentState = EnemyState.Intro;

        Blackbar.GetInstance().Show();

        Parallax.GetInstance().SetBossMode(true);

        HUD.GetInstance().SetupBossBattle(health);
        SwitchState(EnemyState.Outside);

        if (introTimer != null && IsInstanceValid(introTimer))
            introTimer.QueueFree();

        introTimer = new Timer();
        introTimer.WaitTime = introDuration;
        introTimer.OneShot = true;
        introTimer.Timeout += OnIntroFinished;

        Effect.Shake(3, 3);
        AudioManager.GetInstance().Play(AudioManager.AudioType.BossPreExplosion);

        AddChild(introTimer);
        introTimer.Start();
    }

    private void OnIntroFinished()
    {
        if (introTimer != null && IsInstanceValid(introTimer))
        {
            introTimer.Timeout -= OnIntroFinished;
            introTimer.QueueFree();
            introTimer = null;
        }

        musicSync.PlayAll();
        AudioManager.GetInstance().PlayMusic(AudioManager.MusicType.Boss);

        currentAction = ActionState.Idle;

        Blackbar.GetInstance().Hide();
    }

    public override void EntityDeath()
    {
        if (isDeath) return;

        if (currentPhase != PhaseState.Final) return;

        dashTimer?.Stop();
        spawnTimer?.Stop();
        KillActiveTweens();

        musicSync.BeatDetected -= OnMusicBeat;
        AudioManager.GetInstance().StopMusic();

        EndEvent(AudioManager.AudioType.BossExplosion);
        head.CollisionLayer = 0;

        base.EntityDeath();

        Player lPlayer = Player.GetInstance();
        if (lPlayer != null)
        {
            lPlayer.invincible = true;
            lPlayer.isGameWon = true;
        }

        Tween deathTween = CreateTween();
        deathTween.TweenInterval(deathDelayDuration);
        deathTween.Finished += OnDeathSequenceFinished;
    }

    private void OnDeathSequenceFinished()
    {
        CallDeferred(MethodName.CallDeathCleanup);
    }

    private void CallDeathCleanup()
    {
        if (victoryTriggered) return;
        victoryTriggered = true;

        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.Victory);

        ClearAll(false, false);
        Projectiles.ClearAll();
    }

    private void AdvancePhase()
    {
        ulong currentTick = Time.GetTicksMsec();

        if (currentTick - lastPhaseChangeTime < PHASE_COOLDOWN_MS)
        {
            return;
        }

        lastPhaseChangeTime = currentTick;

        if (currentPhase < PhaseState.Final)
            currentPhase++;

        AudioManager.GetInstance().Play(AudioManager.AudioType.BossPreExplosion);

        switch (currentPhase)
        {
            case PhaseState.Intro:
                break;
            case PhaseState.Normal:
                break;
            case PhaseState.Aggressive:
                break;
            case PhaseState.Summoning:
                spawnTimer.Start();
                break;
            case PhaseState.Final:
                spawnTimer.Stop();
                EnterPhaseFinal();
                break;
            default:
                break;
        }
    }

    private void EnterPhaseFinal()
    {
        AudioManager.GetInstance().StopMusic();

        if (Player.GetInstance().isDeath)
            return;

        if (health <= 0)
        {
            EntityDeath();
            int lFinalScore = GetScoreWithCombo(score);
            HUD.GetInstance().CreateBubbleScore(Position, lFinalScore, Colors.DarkOrange);
            return;
        }

        health = 999999;

        SetCollisionMaskValue(1, false);
        SetCollisionMaskValue(9, false);

        EndEvent(AudioManager.AudioType.BossExplosion);
        currentAction = ActionState.Idle;
        shell.SetShellColor(phaseFinalColor);
        SpawnObject(specialSpikeScene, GlobalPosition, true, true);
    }

    private void EndEvent(AudioManager.AudioType pSound)
    {
        invincible = true;
        AudioManager.GetInstance().Play(pSound);
        KillActiveTweens();
    }

    private void SpawnObject(PackedScene pScene, Vector2 pPos, bool pIsSpike = false, bool pPlaySound = false)
    {
        if (pPlaySound) AudioManager.GetInstance().Play(AudioManager.AudioType.BossShoot);

        if (pIsSpike)
        {
            Spike lFinalSpike = Spike.Create(GameManager.GetInstance().gameContainer, pScene, Player.GetInstance(), pPos);
            lFinalSpike.isActive = true;
        }
        else
        {
            Koops lKoops = Koops.Create(pScene, pPos);
            lKoops.Initiate();
        }
    }

    private void SpawnKoops()
    {
        Vector2 lSpawnPos = Camera.GetInstance().GetRandomSpawnPositionRight();
        SpawnObject(koopsScene, lSpawnPos);
    }

    public void SwitchState(EnemyState pNewState)
    {
        currentState = pNewState;
        shellProjectileProtect = (currentState != EnemyState.Outside);

        AnimateParts(currentState);
    }

    private void AnimateParts(EnemyState pState)
    {
        KillActiveTweens();
        visualTween = CreateTween();

        if (pState == EnemyState.Outside)
            AnimateToOutside();
        else
            AnimateToProtected();
    }

    private void AnimateToOutside()
    {
        visualTween.TweenProperty(shell, (string)PropertyName.Position, shellOutsidePos, transitionDuration * TWEEN_OUTSIDE_RATIO)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        visualTween.Parallel().TweenProperty(shell, (string)PropertyName.RotationDegrees, shellOutsideRot, transitionDuration * TWEEN_OUTSIDE_RATIO)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        head.Scale = headRetractedScale;

        visualTween.Parallel().TweenProperty(head, (string)PropertyName.Position, headOutsidePos, transitionDuration)
            .SetDelay(TWEEN_HEAD_DELAY)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);

        visualTween.Parallel().TweenProperty(head, (string)PropertyName.Scale, headOutsideScale, transitionDuration)
            .SetDelay(TWEEN_HEAD_DELAY)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);
    }

    private void AnimateToProtected()
    {
        visualTween.TweenProperty(head, (string)PropertyName.Position, headRetractedPos, transitionDuration * TWEEN_RETRACT_RATIO)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);

        visualTween.Parallel().TweenProperty(head, (string)PropertyName.Scale, headRetractedScale, transitionDuration * TWEEN_RETRACT_RATIO)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.In);

        visualTween.Parallel().TweenProperty(shell, (string)PropertyName.Position, shellRetractedPos, transitionDuration)
            .SetDelay(TWEEN_SHELL_DELAY)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        visualTween.Parallel().TweenProperty(shell, (string)PropertyName.RotationDegrees, shellRetractedRot, transitionDuration)
            .SetDelay(TWEEN_SHELL_DELAY)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        visualTween.Chain().TweenProperty(shell, (string)PropertyName.Scale, Vector2.One * SCALE_POP_FACTOR, TWEEN_SHELL_POP_TIME);
        visualTween.Chain().TweenProperty(shell, (string)PropertyName.Scale, Vector2.One, TWEEN_SHELL_RETURN_TIME);
    }

    private void DoGrooveAnimation()
    {
        if (currentAction != ActionState.Idle || isDeath) return;
        KillActiveTweens();

        beatCounter++;
        float lDir = (beatCounter % BEAT_MODULO == 0) ? GROOVE_DIR_POSITIVE : GROOVE_DIR_NEGATIVE;

        visualTween = CreateTween();
        visualTween.SetParallel(true);

        string lPropertyY = (string)PropertyName.Position + ":y";

        visualTween.TweenProperty(shell, lPropertyY, shellRetractedPos.Y - grooveJumpHeight, grooveStepDuration).SetTrans(Tween.TransitionType.Quad);
        visualTween.TweenProperty(shell, (string)PropertyName.RotationDegrees, shellRetractedRot + (grooveTiltAngle * lDir), grooveStepDuration);
        visualTween.Chain().TweenProperty(shell, lPropertyY, shellRetractedPos.Y, grooveStepDuration).SetTrans(Tween.TransitionType.Quad);
        visualTween.Parallel().TweenProperty(shell, (string)PropertyName.RotationDegrees, shellRetractedRot, grooveReturnDuration);
    }

    private void KillActiveTweens()
    {
        visualTween?.Kill();
    }

    public override void EntityHit(int pDamage)
    {
        if (invincible || isDeath) return;

        base.EntityHit(pDamage);
        HUD.GetInstance().UpdateBossHealth(health);
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        bool lIsHammer = pArea.Owner is Hammer;

        if (lIsHammer)
        {
            currentKnockback = -dashDirection * hammerKnockbackForce;
            SwitchState(EnemyState.Protected);

            Effect.Shake(cameraShakeIntensity, cameraShakeDuration, cameraShakeFrequency, cameraShakeRandomness);
            ScaleTime.SlowMotion();
            Effect.SquashStretch(this, baseScale, false, false, squashStretchDuration);

            HUD.GetInstance().CreateBubbleScore(Position, GetScoreWithCombo(score), Colors.Orange);
        }
    }

    public void OnHeadCollide(Area2D pArea)
    {
        if (isDeath || invincible) return;

        bool lIsHammer = pArea.Owner is Hammer;

        if (lIsHammer)
        {
            currentKnockback = -dashDirection * hammerKnockbackForce;
            SwitchState(EnemyState.Protected);

            Effect.Shake(cameraShakeIntensity, cameraShakeDuration, cameraShakeFrequency, cameraShakeRandomness);
            ScaleTime.SlowMotion();
            Effect.SquashStretch(this, baseScale, false, false, squashStretchDuration);

            HUD.GetInstance().CreateBubbleScore(Position, GetScoreWithCombo(scoreHead), Colors.Red);
        }

        int lDamage = lIsHammer ? Hammer.GetInstance().attackForce : DEFAULT_DAMAGE;

        EntityHit(lDamage);
        shell.ShellDamageAnimation();
    }

    protected override void StartInvincible(float pDuration)
    {
        base.StartInvincible(pDuration);
        shell.SetShellColor(Colors.DarkGray);
    }

    protected override void EndInvincible()
    {
        base.EndInvincible();
        shell.SetShellColor(Colors.Black);
    }
}