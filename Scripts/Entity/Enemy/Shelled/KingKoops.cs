using Godot;
using System.Collections.Generic;

public partial class KingKoops : ShelledEnemy
{
    // --- EXPORTS ---

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
    [Export] private float screenMargin = -250f;

    [ExportCategory("Death Sequence")]
    [Export] private float deathDelayDuration = 3.0f;

    [ExportCategory("Dash Behavior")]
    [Export] private float dashSpeed = 15f;
    [Export] private float dashPhaseSpeedBoost = 2f;
    [Export] private float returnSpeed = 22f;
    [Export] private float returnSpeedScroll = 0.005f;
    [Export] private float dashDuration = 0.5f;
    [Export] private float dashRecoilDuration = 0.5f;
    [Export] private float dashRecoilSpeed = 2f;
    [Export] private float returnThresholdDistance = 20f;
    [Export] private float dashTiltAnglePos = 25f;
    [Export] private float dashTiltAngleNeg = -25f;
    [Export] private float dashTiltDuration = 0.1f;
    [Export] private float returnTiltDuration = 0.2f;

    [ExportCategory("Idle & Visuals")]
    [Export] private float idleLerpSpeedBase = 3f;
    [Export] private float headRotationLerpSpeed = 8f;
    [Export] private float headFlipThreshold = 0.1f;
    [Export] private float shellScaleLerpSpeed = 10f;
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

    // --- ENUMS & STATE ---

    public enum EnemyState { Intro, Outside, Protected }
    private enum ActionState { Idle, PreDash, Dashing, Returning }

    public bool fightActive = false;
    public bool isInvincible = false;

    private EnemyState currentState;
    private ActionState currentAction = ActionState.Idle;

    private int currentPhase = 0;
    private float actionTimer = 0f;
    private int beatCounter = 0;

    private Vector2 movingBasePos;
    private Vector2 dashDirection;

    private Tween visualTween;
    private Tween deathSequenceTween;

    // --- INITIALIZATION ---

    public override void Initiate()
    {
        base.Initiate();
        affectedByScroll = false;

        if (musicSync != null) musicSync.BeatDetected += OnMusicBeat;
        if (head != null) head.AreaEntered += OnHeadCollide;

        StartIntro();
    }

    // --- UPDATE LOOP ---

    protected override void AliveUpdate(float pDelta)
    {
        if (fightActive)
            movingBasePos += GameManager.scrollSpeedVelocity * pDelta;

        if (actionTimer > 0)
            actionTimer -= pDelta;

        HandleActionState(pDelta);
        base.AliveUpdate(pDelta);

        if (fightActive && currentAction == ActionState.Idle)
            GlobalPosition = Utils.ClampToScreen(GlobalPosition, screenMargin, screenMargin);
    }

    // --- DAMAGE & HIT LOGIC ---

    public override void EntityHit(int pDamage)
    {
        // 1. Check Invincibility or Death
        if (isInvincible || isDeath) return;

        base.EntityHit(pDamage);
        HUD.GetInstance().UpdateBossHealth(health);
    }

    public void OnHeadCollide(Area2D pArea)
    {
        if (!fightActive || isInvincible || isDeath) return;

        EntityHit(1);
        shell.ShellDamageAnimation();
    }

    public override void EntityDeath()
    {
        fightActive = false;
        isInvincible = true;
        
        if (head != null) head.CollisionLayer = 0;
        
        base.EntityDeath();

        // 3. Stop Music/Sync
        if (musicSync != null) 
            musicSync.BeatDetected -= OnMusicBeat;

        CallDeferred(MethodName.CallDeathCleanup);

        Timer lTimer = new Timer();
        lTimer.WaitTime = deathDelayDuration;
        lTimer.Timeout += OnDeathSequenceFinished;
        GameManager.GetInstance().gameContainer.AddChild(lTimer);
        lTimer.Start();
    }

    private void OnDeathSequenceFinished()
    {
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.Victory);
        QueueFree();
    }

    private void CallDeathCleanup()
    {
        ClearAll(false);
        Projectiles.ClearAll();
    }

    private void AdvancePhase()
    {
        currentPhase++;
        GD.Print($"BOSS PHASE : {currentPhase}");

        if (currentPhase == 4)
            EnterPhaseFinal();

        else if (currentPhase == 3)
            SpawnKoops();
    }

    private void EnterPhaseFinal()
    {
        isInvincible = true;

        KillActiveTweens();
        currentAction = ActionState.Idle;

        shell.SetShellColor(phaseFinalColor);
        SpecialSpike();
    }

    private void SpecialSpike()
    {
        Spike lSpike = Spike.Create(specialSpikeScene, Player.GetInstance(), GlobalPosition);
        lSpike.isActive = true;
        GameManager.GetInstance().gameContainer.AddChild(lSpike);
    }

    private void HandleActionState(float pDelta)
    {
        switch (currentAction)
        {
            case ActionState.Idle:
                velocity.Y = angleY;
                if (fightActive) UpdateIdlePosition(pDelta);
                break;

            case ActionState.PreDash:
                velocity = -dashDirection * dashRecoilSpeed;
                if (actionTimer <= 0) TriggerDash();
                break;

            case ActionState.Dashing:
                velocity = dashDirection * (dashSpeed + (currentPhase * dashPhaseSpeedBoost));
                if (EndDash()) StartReturn();
                break;

            case ActionState.Returning:
                Vector2 lToHome = movingBasePos - GlobalPosition;
                velocity = lToHome.Normalized() * (returnSpeed + (GameManager.GetInstance().scrollSpeed * returnSpeedScroll));
                if (GlobalPosition.DistanceTo(movingBasePos) < returnThresholdDistance) EndReturn();
                break;
        }
    }

    private void UpdateIdlePosition(float pDelta)
    {
        float lLerpSpeed = idleLerpSpeedBase * currentPhase;
        if (currentPhase >= 4) lLerpSpeed = idleLerpSpeedBase;

        GlobalPosition = GlobalPosition.Lerp(new Vector2(movingBasePos.X, GlobalPosition.Y), pDelta * lLerpSpeed);
        head.Rotation = Mathf.LerpAngle(head.Rotation, 0, pDelta * headRotationLerpSpeed);

        if (Mathf.Abs(head.Rotation) < headFlipThreshold && spriteHead != null)
            spriteHead.FlipV = false;

        if (shell != null && (visualTween == null || !visualTween.IsRunning()))
            shell.Scale = shell.Scale.Lerp(Vector2.One, pDelta * shellScaleLerpSpeed);
    }

    private bool EndDash() => actionTimer <= 0;

    // --- SPAWNING & MUSIC EVENTS ---

    private void SpawnKoops()
    {
        if (koopsScene == null) return;
        Koops lKoops = Koops.Create(koopsScene, GlobalPosition);
        GameManager.GetInstance().gameContainer.AddChild(lKoops);
        lKoops.Initiate();
    }

    private void OnMusicBeat(TickEvent.BeatTypeBoss pType)
    {
        if (!fightActive || isDeath || currentPhase >= 4)
        {
            if (pType == TickEvent.BeatTypeBoss.Move) DoGrooveAnimation();
            return;
        }

        switch (pType)
        {
            case TickEvent.BeatTypeBoss.Phase: AdvancePhase(); break;
            case TickEvent.BeatTypeBoss.Shoot: if (currentAction == ActionState.Idle) TryFireRandomSpike(); break;
            case TickEvent.BeatTypeBoss.Reload: if (currentAction == ActionState.Idle) SpawnSpikes(); break;
            case TickEvent.BeatTypeBoss.Dash: if (currentAction == ActionState.Idle) StartDashSequence(); break;
            case TickEvent.BeatTypeBoss.Move: if (currentAction == ActionState.Idle) DoGrooveAnimation(); break;
        }
    }

    // --- COMBAT BEHAVIORS ---

    private void StartDashSequence()
    {
        KillActiveTweens();
        currentAction = ActionState.PreDash;
        actionTimer = dashRecoilDuration;
        affectedByScroll = false;
        facePlayer = false;
        currentState = EnemyState.Protected;
        ApplyTransformInstant(EnemyState.Protected);

        if (currentPhase == 3) SpawnKoops();

        Vector2 lPlayerPos = Player.GetInstance().GlobalPosition;
        dashDirection = Utils.DirectionTo(GlobalPosition, lPlayerPos);
        
        Flip(dashDirection.X < 0);
        
        head.GlobalRotationDegrees = Utils.LookAt(lPlayerPos, head.GlobalPosition);
        
        if (spriteHead != null) 
            spriteHead.FlipV = (dashDirection.X < 0);
    }

    private void TriggerDash()
    {
        KillActiveTweens();
        currentAction = ActionState.Dashing;
        actionTimer = dashDuration;
        
        SwitchState(EnemyState.Outside, true);
        
        float lTilt = dashDirection.X < 0 ? dashTiltAnglePos : dashTiltAngleNeg;
        CreateTween().TweenProperty(shell, (string)Node2D.PropertyName.RotationDegrees, lTilt, dashTiltDuration);
    }

    private void StartReturn()
    {
        KillActiveTweens();
        currentAction = ActionState.Returning;
        SwitchState(EnemyState.Protected, false);
        CreateTween().TweenProperty(shell, (string)Node2D.PropertyName.RotationDegrees, shellRetractedRot, returnTiltDuration);
    }

    private void EndReturn()
    {
        KillActiveTweens();
        currentAction = ActionState.Idle;
        velocity = Vector2.Zero;
        GlobalPosition = movingBasePos;
        affectedByScroll = true;
        ApplyTransformInstant(EnemyState.Protected);
    }

    // --- VISUAL STATES & ANIMATION ---

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
        var lEase = Tween.EaseType.Out;

        visualTween.TweenProperty(head, (string)Node2D.PropertyName.Position, lIsOutside ? headOutsidePos : headRetractedPos, transitionDuration)
            .SetTrans(lTrans).SetEase(lEase);
        
        visualTween.TweenProperty(shell, (string)Node2D.PropertyName.Position, lIsOutside ? shellOutsidePos : shellRetractedPos, transitionDuration)
            .SetTrans(lTrans).SetEase(lEase);
        
        visualTween.TweenProperty(shell, (string)Node2D.PropertyName.RotationDegrees, lIsOutside ? shellOutsideRot : shellRetractedRot, transitionDuration)
            .SetTrans(lTrans).SetEase(lEase);
    }

    private void DoGrooveAnimation()
    {
        if (currentAction != ActionState.Idle || isDeath) return;
        
        KillActiveTweens();
        beatCounter++;
        float lDir = (beatCounter % 2 == 0) ? 1f : -1f;
        
        visualTween = CreateTween().SetParallel(true);

        // Jump Up
        visualTween.TweenProperty(shell, (string)Node2D.PropertyName.Position + ":y", shellRetractedPos.Y - grooveJumpHeight, grooveStepDuration)
            .SetTrans(Tween.TransitionType.Quad);
        
        // Tilt
        visualTween.TweenProperty(shell, (string)Node2D.PropertyName.RotationDegrees, shellRetractedRot + (grooveTiltAngle * lDir), grooveStepDuration);
        
        // Land
        visualTween.Chain().TweenProperty(shell, (string)Node2D.PropertyName.Position + ":y", shellRetractedPos.Y, grooveStepDuration)
            .SetTrans(Tween.TransitionType.Quad);
        
        // Return Tilt
        visualTween.Parallel().TweenProperty(shell, (string)Node2D.PropertyName.RotationDegrees, shellRetractedRot, grooveReturnDuration);
    }

    private void KillActiveTweens()
    {
        if (visualTween != null && visualTween.IsRunning()) visualTween.Kill();
    }

    private void StartIntro()
    {
        currentState = EnemyState.Intro;
        HUD.GetInstance().SetupBossBattle(health);
        SwitchState(EnemyState.Protected, true);
        
        CreateTween().TweenInterval(introDuration).Finished += OnIntroFinished;
    }

    private void OnIntroFinished()
    {
        movingBasePos = GlobalPosition;
        fightActive = true;
        affectedByScroll = true;
        if (musicSync != null) musicSync.PlayAll();
    }
}