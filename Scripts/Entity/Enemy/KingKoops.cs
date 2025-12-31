using Godot;
using System.Collections.Generic;

public partial class KingKoops : ShelledEnemy
{
    [ExportGroup("References")]
    [Export] private Area2D head;
    [Export] private Sprite2D spriteHead;

    public enum EnemyState { Intro, Outside, Protected }
    public enum BossPhase { A, B, C }

    [ExportGroup("State & Logic")]
    public bool fightActive = false;
    private EnemyState currentState;
    public BossPhase currentPhase = BossPhase.A;
    private float introCurrentX;

    [ExportGroup("Dash Settings")]
    [Export] private float dashSpeed = 400f;
    [Export] private float dashDuration = 0.8f;
    [Export] private float dashRecoilDuration = 1.0f;
    [Export] private float dashRecoilDist = -50f;

    [Export] private float phaseBCooldown = 1.5f;
    [Export] private float phaseCCooldown = 0.5f;

    private bool isDashing = false;
    private bool isPreparingDash = false;
    private Timer dashDurationTimer;
    private Timer dashCooldownTimer;
    private Vector2 dashDirection;

    private bool phaseBNextActionIsDash = false;

    private const float SCREEN_MARGIN = -250f;

    [ExportGroup("Spike Settings")]
    [Export] private int numSpikes = 8;
    [Export] private PackedScene spikeScene;
    [Export] private Node2D spikeContainer;

    [ExportGroup("Shooting Settings")]
    [Export] private float fireRate = 2.5f;
    [Export] private float shotPreDelay = 0.5f;
    [Export] private float spikeAdvanceMultiplier = 1.3f;
    [Export] private float sizeMultiplier = 1.2f;

    private Timer shotTimer;
    private Tween fireTween;
    private List<Spike> spikesList = new List<Spike>();
    private int lastShotIndex = -1;

    [ExportGroup("Intro Settings")]
    [Export] private float introTargetOffsetX = 800f;
    [Export] private float introDuration = 3.5f;
    [Export] private float transitionDuration = 0.4f;

    [ExportSubgroup("Pos Settings")]
    [Export] private Vector2 headRetractedPos = Vector2.Zero;
    [Export] private Vector2 headRetractedScale = Vector2.One;
    [Export] private Vector2 shellRetractedPos = Vector2.Zero;
    [Export] private float shellRetractedRot = 0f;
    [Export] private Vector2 headOutsidePos = new Vector2(15f, 0f);
    [Export] private Vector2 headOutsideScale = Vector2.One;
    [Export] private Vector2 shellOutsidePos = new Vector2(0f, -20f);
    [Export] private float shellOutsideRot = -25f;

    private Tween stateTween;
    private Tween introTween;

    public override void Initiate()
    {
        GD.Print("KingKoops: Initiate called");
        base.Initiate();
        affectedByScroll = false;

        shotTimer = new Timer { WaitTime = fireRate, OneShot = false, Autostart = false };
        shotTimer.Timeout += OnPhaseAShotTimer;
        AddChild(shotTimer);

        dashDurationTimer = new Timer { WaitTime = dashDuration, OneShot = true };
        dashDurationTimer.Timeout += EndDashMovement;
        AddChild(dashDurationTimer);

        dashCooldownTimer = new Timer { OneShot = true };
        dashCooldownTimer.Timeout += OnCooldownTimerTimeout;
        AddChild(dashCooldownTimer);

        if (shell != null)
            shell.SetShellColor(Colors.Black);

        StartIntro();
    }

    protected override void AliveUpdate(float pDelta)
    {
        if (isDashing)
            velocity = dashDirection * dashSpeed;

        else if (isPreparingDash)
            velocity = Vector2.Zero;

        else
        {
            velocity.Y = angleY;

            if (fightActive)
            {
                head.Rotation = Mathf.LerpAngle(head.Rotation, 0, pDelta * 5f);

                if (Mathf.Abs(head.Rotation) < 0.1f && spriteHead != null)
                    spriteHead.FlipV = false;
            }
        }

        base.AliveUpdate(pDelta);

        if (currentState == EnemyState.Intro)
            GlobalPosition = new Vector2(introCurrentX, GlobalPosition.Y);

        if (fightActive || currentState == EnemyState.Intro)
        {
            GlobalPosition = Utils.ClampToScreen(GlobalPosition, SCREEN_MARGIN, SCREEN_MARGIN);
        }

        if (!fightActive) return;

        // DEBUG INPUTS
        if (Input.IsActionJustPressed(Property.INPUT_PHASEA)) ChangePhase(BossPhase.A);
        if (Input.IsActionJustPressed(Property.INPUT_PHASEB)) ChangePhase(BossPhase.B);
        if (Input.IsActionJustPressed(Property.INPUT_PHASEC)) ChangePhase(BossPhase.C);

        if (isDashing || isPreparingDash) return;

        if (Input.IsActionJustPressed(Property.INPUT_BOSSNORMAL)) SwitchState(EnemyState.Outside);
        if (Input.IsActionJustPressed(Property.INPUT_BOSSPROTECTED)) SwitchState(EnemyState.Protected);
    }


    private void StartDashSequence()
    {
        if (isDeath) return;

        GD.Print("KingKoops: Starting Dash Sequence (Recoil)");
        affectedByScroll = false;
        isPreparingDash = true;
        isDashing = false;
        facePlayer = false;
        shellProjectileProtect = true;
        SwitchState(EnemyState.Protected);

        wing.SetSpeedDouble(true);

        Vector2 playerPos = Player.GetInstance().GlobalPosition;

        dashDirection = Utils.DirectionTo(GlobalPosition, playerPos);

        bool isLookingLeft = dashDirection.X < 0;

        Flip(isLookingLeft);

        float angleToPlayer = Utils.LookAt(playerPos, head.GlobalPosition);
        head.GlobalRotationDegrees = angleToPlayer;

        spriteHead.FlipV = (dashDirection.X < 0);

        float currentRecoilDist = isLookingLeft ? -dashRecoilDist : dashRecoilDist;

        Vector2 lRecoilPos = Position - dashDirection * currentRecoilDist;

        Tween lTween = CreateTween();
        lTween.TweenProperty(this, "position", lRecoilPos, dashRecoilDuration)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);

        lTween.Finished += TriggerDashMovement;
    }

    private void TriggerDashMovement()
    {
        if (isDeath) return;
        GD.Print("KingKoops: Dash GO! Head OUT");
        isPreparingDash = false;
        isDashing = true;

        SwitchState(EnemyState.Outside); // Sort la tête orientée

        dashDurationTimer.Start();
    }

    private void EndDashMovement()
    {
        GD.Print("KingKoops: Dash Ended. Head IN");
        isDashing = false;

        SwitchState(EnemyState.Protected); // Rentre la tête

        wing.SetSpeedDouble(false);
        velocity = Vector2.Zero;
        affectedByScroll = true;

        // Reset Timer selon phase
        float cooldown = (currentPhase == BossPhase.B) ? phaseBCooldown : phaseCCooldown;
        if (currentPhase == BossPhase.B) phaseBNextActionIsDash = false;

        dashCooldownTimer.WaitTime = cooldown;
        dashCooldownTimer.Start();
    }

    // --- PHASE MANAGEMENT ---

    public void ChangePhase(BossPhase newPhase)
    {
        GD.Print($"KingKoops: Changing Phase to {newPhase}");
        currentPhase = newPhase;

        shotTimer.Stop();
        dashCooldownTimer.Stop();
        dashDurationTimer.Stop();
        isDashing = false;
        isPreparingDash = false;

        SwitchState(EnemyState.Protected);

        switch (currentPhase)
        {
            case BossPhase.A: shotTimer.Start(); break;
            case BossPhase.B: ActivateRandomSpike(); break;
            case BossPhase.C:
                dashCooldownTimer.WaitTime = 0.5f;
                dashCooldownTimer.Start();
                break;
        }
    }

    private void OnCooldownTimerTimeout()
    {
        GD.Print("KingKoops: Cooldown Timeout");
        if (currentPhase == BossPhase.C)
        {
            StartDashSequence();
        }
        else if (currentPhase == BossPhase.B)
        {
            if (phaseBNextActionIsDash) StartDashSequence();
            else ActivateRandomSpike();
        }
    }

    // --- SPIKES LOGIC ---

    private void OnPhaseAShotTimer()
    {
        if (currentPhase == BossPhase.A) ActivateRandomSpike();
    }

    private void SpawnSpikes()
    {
        if (spikeContainer == null) return;
        GD.Print("KingKoops: Spawning Spikes");

        int lRadius = 155;
        float lStartAngle = -Mathf.Pi / 6;
        float lEndAngle = 5 * -Mathf.Pi / 6;
        float lAngleStep = (lEndAngle - lStartAngle) / (numSpikes - 1);

        for (int i = 0; i < numSpikes; i++)
        {
            if (i < spikesList.Count && spikesList[i] != null && !spikesList[i].isActive)
                continue;

            float lAngle = lStartAngle + (i * lAngleStep);
            Vector2 lLocalPosition = new Vector2(Mathf.Cos(lAngle) * lRadius, Mathf.Sin(lAngle) * lRadius);

            Spike lSpike = Spike.Create(spikeScene, Player.GetInstance(), lLocalPosition);
            lSpike.Rotation = lAngle;

            if (i < spikesList.Count) spikesList[i] = lSpike;
            else spikesList.Add(lSpike);

            spikeContainer.AddChild(lSpike);
            lSpike.lifeTimer.Stop();

            CreateTween().TweenProperty(lSpike, "position", lLocalPosition * spikeAdvanceMultiplier, shotPreDelay)
                  .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.InOut);
        }
    }

    private void ActivateRandomSpike()
    {
        if (!fightActive || spikesList.Count == 0) return;
        if (spikesList.Count < numSpikes) SpawnSpikes();

        int lIndex;
        int attempts = 0;
        do
        {
            lIndex = GameManager.rand.RandiRange(0, spikesList.Count - 1);
            attempts++;
        } while ((lIndex == lastShotIndex || spikesList[lIndex] == null || spikesList[lIndex].isActive) && attempts < 10);

        lastShotIndex = lIndex;
        GD.Print($"KingKoops: Selected spike index {lIndex}");
        FireSpike(spikesList[lIndex]);
    }

    private void FireSpike(Spike pSpike)
    {
        if (isDeath) return;
        GD.Print("KingKoops: Charging Spike");

        fireTween = CreateTween();
        fireTween.TweenProperty(pSpike.sprite, "self_modulate", Colors.Red, shotPreDelay)
             .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.InOut);

        fireTween.Finished += () => LaunchSpike(pSpike);
    }

    private void LaunchSpike(Spike pSpike)
    {
        if (pSpike.isActive) return;
        GD.Print("KingKoops: Launching Spike!");
        pSpike.isActive = true;

        Vector2 lGlobalPos = pSpike.GlobalPosition;
        float lGlobalRot = pSpike.GlobalRotation;

        spikeContainer.RemoveChild(pSpike);
        GameManager.GetInstance().gameContainer.AddChild(pSpike);

        pSpike.GlobalPosition = lGlobalPos;
        pSpike.GlobalRotation = lGlobalRot;

        pSpike.lifeTimer.Start();
        CreateTween().TweenProperty(pSpike, "scale", pSpike.Scale * sizeMultiplier, 0.5f);
        Projectiles.enemyProjectilesList.Add(pSpike);
        pSpike.particles.Emitting = true;
        pSpike.collision.Disabled = false;

        SpawnSpikes();

        if (currentPhase == BossPhase.B)
        {
            phaseBNextActionIsDash = true;
            dashCooldownTimer.WaitTime = phaseBCooldown;
            dashCooldownTimer.Start();
        }
    }

    // --- INTRO & STATES ---

    private void StartIntro()
    {
        GD.Print("KingKoops: Starting Intro");
        currentState = EnemyState.Intro;
        fightActive = false;
        SwitchState(EnemyState.Protected, true);

        introCurrentX = GlobalPosition.X;
        float lTargetX = Camera.GetInstance().GlobalPosition.X + introTargetOffsetX;

        if (introTween != null && introTween.IsRunning()) introTween.Kill();
        introTween = CreateTween();
        introTween.TweenProperty(this, nameof(introCurrentX), lTargetX, introDuration)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        introTween.Finished += OnIntroFinished;
    }

    private void OnIntroFinished()
    {
        GD.Print("KingKoops: Intro Finished");
        fightActive = true;
        affectedByScroll = true;
        velocity = Vector2.Zero;
        SpawnSpikes();
        ChangePhase(BossPhase.A);
    }

    public override void EntityHit(int pDamage)
    {
        base.EntityHit(pDamage);
        GD.Print($"KingKoops: Hit! Damage: {pDamage}");
        if (currentPhase == BossPhase.A && spikesList.Count > 0 && fightActive)
            ActivateRandomSpike();
    }

    public override void EntityDeath()
    {
        GD.Print("KingKoops: Death");
        base.EntityDeath();
        shotTimer.Stop();
        dashDurationTimer.Stop();
        dashCooldownTimer.Stop();
        if (fireTween != null) fireTween.Kill();
    }

    public void SwitchState(EnemyState pNewState, bool instant = false)
    {
        if (currentState == pNewState && !instant) return;
        GD.Print($"KingKoops: Switching State to {pNewState}");
        currentState = pNewState;

        shellProjectileProtect = (currentState != EnemyState.Outside);

        if (instant) ApplyTransformInstant(currentState);
        else AnimateParts(currentState);
    }

    private void ApplyTransformInstant(EnemyState state)
    {
        bool outside = (state == EnemyState.Outside);
        head.Position = outside ? headOutsidePos : headRetractedPos;
        head.Scale = outside ? headOutsideScale : headRetractedScale;
        shell.Position = outside ? shellOutsidePos : shellRetractedPos;
        shell.RotationDegrees = outside ? shellOutsideRot : shellRetractedRot;
    }

    private void AnimateParts(EnemyState state)
    {
        if (stateTween != null && stateTween.IsRunning()) stateTween.Kill();

        bool outside = (state == EnemyState.Outside);
        stateTween = CreateTween().SetParallel(true);

        stateTween.TweenProperty(head, "position", outside ? headOutsidePos : headRetractedPos, transitionDuration)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        stateTween.TweenProperty(head, "scale", outside ? headOutsideScale : headRetractedScale, transitionDuration)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        stateTween.TweenProperty(shell, "position", outside ? shellOutsidePos : shellRetractedPos, transitionDuration)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        stateTween.TweenProperty(shell, "rotation_degrees", outside ? shellOutsideRot : shellRetractedRot, transitionDuration)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
    }

    public void OnHeadCollide(Area2D pArea)
    {
        if (!fightActive) return;
        EntityHit(1);
    }
}
