using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;
using Com.IsartDigital.Shmup.NHUD;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NMusic;
using Com.IsartDigital.Shmup.NToolsBox;
using Godot;

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

    // A changer un peu le b
    // A changer un peu le b
    // A changer un peu le b
    // A changer un peu le b
    [Export] private float preDashDurationA = 0.5f;
    [Export] private float dashDurationA = 0.5f;
    [Export] private float preDashDurationB = 0.5f;
    [Export] private float dashDurationB = 0.5f;

    [Export] private float recoveryDuration = 1.0f;
    [Export] private float dashSpeed = 800f;
    [Export] private float recoilDistance = 50f;
    [Export] private float deceleration = 5f;

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

    public bool isInvincible = false;

    private Timer dashTimer;
    private Vector2 dashDirection;
    private int beatCounter = 0;

    private Tween visualTween;
    private Timer animationTimer;

    public override void Initiate()
    {
        base.Initiate();

        SwitchState(EnemyState.Protected);

        musicSync.BeatDetected += OnMusicBeat;
        head.AreaEntered += OnHeadCollide;

        dashTimer = new Timer();
        dashTimer.OneShot = true;
        AddChild(dashTimer);

        StartIntro();
    }

    protected override void AliveUpdate(float pDelta)
    {
        spriteHead.RotationDegrees = Utils.LookAt(playerPos, Position);

        if (currentAction != ActionState.Idle)
            UpdateDashLogic(pDelta);
        else
            PhaseAction(pDelta);

        base.AliveUpdate(pDelta);
        
        Position = Utils.ClampToScreen(Position, 150f);
    }

    private void StartDashSequence()
    {
        if (currentAction != ActionState.Idle) return;

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

        visualTween.Parallel().TweenProperty(this, (string)PropertyName.GlobalPosition, lRecoilPos, preDashDuration)
                      .SetTrans(Tween.TransitionType.Back)
                      .SetEase(Tween.EaseType.Out);

        shell.SetShellColor(Colors.DarkRed);

        if (currentPhase == PhaseState.Summoning)
            SpawnObject(koopsScene, GlobalPosition);
    }

    private void ExecuteDash()
    {
        dashTimer.Timeout -= ExecuteDash;

        currentAction = ActionState.Dashing;
        dashDirection = playerDirection.Normalized();

        SwitchState(EnemyState.Outside);

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
        switch (currentAction)
        {
            case ActionState.PreDash:
                velocity = Vector2.Zero;
                break;

            case ActionState.Dashing:
                velocity = dashDirection * dashSpeed;
                break;

            case ActionState.Recovery:
                velocity = velocity.Lerp(Vector2.Zero, deceleration * pDelta);
                break;
        }
    }

    private void PhaseAction(float pDelta)
    {
        Vector2 lCenter = Camera.GetInstance().GetScreenCenter();

        velocity.Y = angleY;

        switch (currentPhase)
        {
            case PhaseState.Normal:
                if (Utils.IsOutOfBound(Position, -150, true))
                {
                    if (Position.X < lCenter.X)
                    {
                        direction = MoveDirection.Right;
                        velocity.X = 1;
                    }
                    else if (Position.X > lCenter.X)
                    {
                        direction = MoveDirection.Left;
                        velocity.X = -1;
                    }
                }
                break;

            case PhaseState.Aggressive:
                velocity = playerDirection;
                break;

            case PhaseState.Summoning:
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

        HUD.GetInstance().SetupBossBattle(health);
        SwitchState(EnemyState.Protected);

        animationTimer = new Timer();
        animationTimer.WaitTime = introDuration;
        animationTimer.Timeout += OnIntroFinished;

        GameManager.GetInstance().gameContainer.AddChild(animationTimer);
        animationTimer.Start();
    }

    private void OnIntroFinished()
    {
        animationTimer.Stop();
        musicSync.PlayAll();
        AudioManager.GetInstance().PlayMusic(AudioManager.MusicType.Boss);

        currentAction = ActionState.Idle;
    }

    private void AdvancePhase()
    {
        if (currentPhase < PhaseState.Final)
            currentPhase++;

        AudioManager.GetInstance().Play(AudioManager.AudioType.BossPreExplosion);

        switch (currentPhase)
        {
            case PhaseState.Intro:
                break;
            case PhaseState.Normal:
                affectedByScroll = true;
                break;
            case PhaseState.Final:
                EnterPhaseFinal();
                break;
            default:
                break;
        }
    }

    private void EnterPhaseFinal()
    {
        if (health <= 0)
        {
            EntityDeath();
            return;
        }

        EndEvent(AudioManager.AudioType.BossExplosion);
        currentAction = ActionState.Idle;
        shell.SetShellColor(phaseFinalColor);
        SpawnObject(specialSpikeScene, GlobalPosition, true, true);
    }

    private void EndEvent(AudioManager.AudioType pSound)
    {
        isInvincible = true;
        AudioManager.GetInstance().Play(pSound);
        AudioManager.GetInstance().StopMusic();
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
        visualTween.TweenProperty(shell, (string)Node2D.PropertyName.Position, shellOutsidePos, transitionDuration * 0.6f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        visualTween.Parallel().TweenProperty(shell, (string)Node2D.PropertyName.RotationDegrees, shellOutsideRot, transitionDuration * 0.6f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        head.Scale = headRetractedScale;

        visualTween.Parallel().TweenProperty(head, (string)Node2D.PropertyName.Position, headOutsidePos, transitionDuration)
            .SetDelay(0.1f)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);

        visualTween.Parallel().TweenProperty(head, (string)Node2D.PropertyName.Scale, headOutsideScale, transitionDuration)
            .SetDelay(0.1f)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);
    }

    private void AnimateToProtected()
    {
        visualTween.TweenProperty(head, (string)Node2D.PropertyName.Position, headRetractedPos, transitionDuration * 0.5f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);

        visualTween.Parallel().TweenProperty(head, (string)Node2D.PropertyName.Scale, headRetractedScale, transitionDuration * 0.5f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.In);

        visualTween.Parallel().TweenProperty(shell, (string)Node2D.PropertyName.Position, shellRetractedPos, transitionDuration)
            .SetDelay(0.05f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        visualTween.Parallel().TweenProperty(shell, (string)Node2D.PropertyName.RotationDegrees, shellRetractedRot, transitionDuration)
            .SetDelay(0.05f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        visualTween.Chain().TweenProperty(shell, (string)Node2D.PropertyName.Scale, Vector2.One * 1.1f, 0.05f);
        visualTween.Chain().TweenProperty(shell, (string)Node2D.PropertyName.Scale, Vector2.One, 0.1f);
    }

    private void DoGrooveAnimation()
    {
        if (currentAction != ActionState.Idle || isDeath) return;
        KillActiveTweens();

        beatCounter++;
        float lDir = (beatCounter % 2 == 0) ? 1f : -1f;

        visualTween = CreateTween();
        visualTween.SetParallel(true);
        visualTween.TweenProperty(shell, (string)PropertyName.Position + ":y", shellRetractedPos.Y - grooveJumpHeight, grooveStepDuration).SetTrans(Tween.TransitionType.Quad);
        visualTween.TweenProperty(shell, (string)PropertyName.RotationDegrees, shellRetractedRot + (grooveTiltAngle * lDir), grooveStepDuration);
        visualTween.Chain().TweenProperty(shell, (string)PropertyName.Position + ":y", shellRetractedPos.Y, grooveStepDuration).SetTrans(Tween.TransitionType.Quad);
        visualTween.Parallel().TweenProperty(shell, (string)PropertyName.RotationDegrees, shellRetractedRot, grooveReturnDuration);
    }

    private void KillActiveTweens()
    {
        visualTween?.Kill();
    }

    public override void EntityHit(int pDamage)
    {
        if (isInvincible || isDeath) return;
        base.EntityHit(pDamage);
        HUD.GetInstance().UpdateBossHealth(health);
    }

    public void OnHeadCollide(Area2D pArea)
    {
        if (isInvincible || isDeath) return;
        int lDamage = (pArea.Owner is Hammer) ? Hammer.GetInstance().attackForce : 1;
        EntityHit(lDamage);
        shell.ShellDamageAnimation();
    }

    public override void EntityDeath()
    {
        if (currentPhase != PhaseState.Final) return;

        dashTimer?.Stop();
        KillActiveTweens();

        EndEvent(AudioManager.AudioType.BossExplosion);
        head.CollisionLayer = 0;
        base.EntityDeath();

        musicSync.BeatDetected -= OnMusicBeat;
        CallDeferred(MethodName.CallDeathCleanup);
        AudioManager.GetInstance().StopMusic();
        Player.GetInstance().invincible = true;

        animationTimer.WaitTime = deathDelayDuration;
        animationTimer.Timeout += OnDeathSequenceFinished;
        animationTimer.Start();
    }

    private void OnDeathSequenceFinished()
    {
        CallDeferred(MethodName.CallDeathCleanup);
        animationTimer.Stop();
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.Victory);
        AudioManager.GetInstance().StopMusic();
        QueueFree();
    }

    private void CallDeathCleanup()
    {
        ClearAll(false, false);
        Projectiles.ClearAll();
    }
}