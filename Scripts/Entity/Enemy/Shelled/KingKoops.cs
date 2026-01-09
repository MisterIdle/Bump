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
    [Export] private float dashSpeed = 100f;
    [Export] private float dashDuration = 0.1f;
    [Export] private float dashRecoilDuration = 0.5f;
    [Export] private float dashRecoilDistance = 50f;

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
    private enum ActionState { Idle, PreDash, Dashing, Returning }
    private enum PhaseState { Intro, Normal, Aggressive, Summoning, Final }

    private EnemyState currentState;
    private ActionState currentAction = ActionState.Idle;
    private PhaseState currentPhase = PhaseState.Intro;

    public bool isInvincible = false;

    private Timer actionTimer;
    private int beatCounter = 0;
    private Vector2 dashDirection;

    private Tween visualTween;
    private Timer animationTimer;

    public override void Initiate()
    {
        base.Initiate();

        SwitchState(EnemyState.Protected, true);

        musicSync.BeatDetected += OnMusicBeat;

        head.AreaEntered += OnHeadCollide;

        StartIntro();
    }

    protected override void AliveUpdate(float pDelta)
    {
        if (currentAction != ActionState.PreDash)
            Action(pDelta);

        PhaseAction(pDelta);

        base.AliveUpdate(pDelta);
    }

    private void PhaseAction(float pDelta)
    {
        if (currentAction != ActionState.Idle) return;

        Vector2 lCenter = Camera.GetInstance().GetScreenCenter();

        switch (currentPhase)
        {
            case PhaseState.Normal:
                if (Utils.IsOutOfBound(Position, -150, true))
                {
                    if (Position.X < lCenter.X)
                    {
                        direction = MoveDirection.Right;
                        velocity.X = 1;
                        affectedByScroll = true;
                    }
                    else if (Position.X > lCenter.X)
                    {
                        direction = MoveDirection.Left;
                        velocity.X = -1;
                        affectedByScroll = false;
                    }
                }
                break;

            case PhaseState.Aggressive:
                Vector2 lTargetPos = new Vector2(lCenter.X, Position.Y);
                velocity = Vector2.Zero;
                Position = Position.Lerp(lTargetPos, 2.0f * pDelta);
                break;

            case PhaseState.Summoning:
            case PhaseState.Final:
                velocity.X = 0;
                break;
        }
    }

    private void OnMusicBeat(TickEvent.BeatTypeBoss pType)
    {
        if (currentAction != ActionState.Idle && currentAction != ActionState.Returning) return;

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
                break;

            case TickEvent.BeatTypeBoss.Move:
                DoGrooveAnimation();
                break;
        }
    }

    private void Action(float pDelta)
    {
        switch (currentAction)
        {
            case ActionState.Idle:
                velocity.Y = angleY;
                break;

            case ActionState.Dashing:
                velocity = dashDirection * dashSpeed * pDelta;
                break;
        }
    }

    private void StartIntro()
    {
        AudioManager.GetInstance().StopMusic();
        currentState = EnemyState.Intro;

        HUD.GetInstance().SetupBossBattle(health);
        SwitchState(EnemyState.Protected, true);

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

        if (currentPhase == PhaseState.Final) 
            EnterPhaseFinal();

        if (currentPhase == PhaseState.Summoning)
            SpawnObject(koopsScene, GlobalPosition);
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
            Spike lFinalSpike = Spike.Create(spikeContainer, pScene, Player.GetInstance(), pPos);
            lFinalSpike.isActive = true;
        }
        else
        {
            Koops lKoops = Koops.Create(pScene, pPos);
            lKoops.Initiate();
        }
    }

    public void SwitchState(EnemyState pNewState, bool pInstant = false)
    {
        if (currentState == pNewState && !pInstant) return;

        currentState = pNewState;
        shellProjectileProtect = (currentState != EnemyState.Outside);

        if (pInstant) ApplyTransformInstant(currentState);
        else AnimateParts(currentState);
    }

    private void ApplyTransformInstant(EnemyState pState)
    {
        bool lIsOutside = (pState == EnemyState.Outside);
        head.Position = lIsOutside ? headOutsidePos : headRetractedPos;
        head.Scale = lIsOutside ? headOutsideScale : headRetractedScale;
        shell.Position = lIsOutside ? shellOutsidePos : shellRetractedPos;
        shell.RotationDegrees = lIsOutside ? shellOutsideRot : shellRetractedRot;
    }

    private void AnimateParts(EnemyState pState)
    {
        KillActiveTweens();
        bool lIsOutside = (pState == EnemyState.Outside);

        visualTween = CreateTween().SetParallel(true);
        var lTrans = Tween.TransitionType.Back;
        var lEase = lIsOutside ? Tween.EaseType.Out : Tween.EaseType.In;

        visualTween.TweenProperty(head, (string)Node2D.PropertyName.Position, lIsOutside ? headOutsidePos : headRetractedPos, transitionDuration).SetTrans(lTrans).SetEase(lEase);
        visualTween.TweenProperty(head, (string)Node2D.PropertyName.Scale, lIsOutside ? headOutsideScale : headRetractedScale, transitionDuration).SetTrans(lTrans).SetEase(lEase);
        visualTween.TweenProperty(shell, (string)Node2D.PropertyName.Position, lIsOutside ? shellOutsidePos : shellRetractedPos, transitionDuration).SetTrans(lTrans).SetEase(lEase);
        visualTween.TweenProperty(shell, (string)Node2D.PropertyName.RotationDegrees, lIsOutside ? shellOutsideRot : shellRetractedRot, transitionDuration).SetTrans(lTrans).SetEase(lEase);
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

        actionTimer?.Stop();
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