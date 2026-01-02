using Godot;
using System.Collections.Generic;

public partial class KingKoops : ShelledEnemy
{
    [ExportGroup("References")]
    [Export] private Area2D head;
    [Export] private Sprite2D spriteHead;
    [Export] private TickEvent musicSync;
    [Export] private Node2D spikeContainer;
    [Export] private PackedScene spikeScene;

    [ExportGroup("Combat Stats")]
    [Export] private float introDuration = 2.0f;
    [Export] private float transitionDuration = 0.4f;

    [ExportGroup("Dash Behavior")]
    [Export] private float dashSpeed = 15f;
    [Export] private float returnSpeed = 22f;
    [Export] private float dashDuration = 0.5f;
    [Export] private float dashRecoilDuration = 0.5f;
    [Export] private float dashRecoilSpeed = 2f;

    [ExportGroup("Spike Attack")]
    [Export] private int numSpikes = 8;
    [Export] private float shotPreDelay = 0.2f;
    [Export] private float spikeAdvanceMultiplier = 1.3f;
    [Export] private float spikeSizeMultiplier = 1.2f;

    [ExportGroup("Visuals")]
    [Export] private float grooveJumpHeight = 25f;
    [Export] private float grooveTiltAngle = 8f;

    [ExportSubgroup("Transform Targets")]
    [Export] private Vector2 headRetractedPos = Vector2.Zero;
    [Export] private Vector2 headRetractedScale = Vector2.One;
    [Export] private Vector2 headOutsidePos = new Vector2(15f, 0f);
    [Export] private Vector2 headOutsideScale = Vector2.One;

    [Export] private Vector2 shellRetractedPos = Vector2.Zero;
    [Export] private float shellRetractedRot = 0f;
    [Export] private Vector2 shellOutsidePos = new Vector2(0f, -20f);
    [Export] private float shellOutsideRot = -25f;

    public enum EnemyState { Intro, Outside, Protected }
    private enum ActionState { Idle, PreDash, Dashing, Returning }

    public bool fightActive = false;
    private EnemyState currentState;
    private ActionState currentAction = ActionState.Idle;

    private float actionTimer = 0f;
    private int beatCounter = 0;
    private Vector2 movingBasePos;
    private Vector2 dashDirection;

    private Tween visualTween;
    private Tween fireTween;

    private List<Spike> spikesList = new List<Spike>();
    private const float SCREEN_MARGIN = -250f;
    public override void Initiate()
    {
        base.Initiate();

        affectedByScroll = false;
        if (shell != null) shell.SetShellColor(Colors.Black);

        musicSync.BeatDetected += OnMusicBeat;
        head.AreaEntered += OnHeadCollide;

        StartIntro();
    }

    private void StartIntro()
    {
        currentState = EnemyState.Intro;
        fightActive = false;

        HUD.GetInstance().SetupBossBattle(health);

        SwitchState(EnemyState.Protected, true);

        CreateTween().TweenInterval(introDuration).Finished += OnIntroFinished;
    }

    private void OnIntroFinished()
    {
        movingBasePos = GlobalPosition;
        currentState = EnemyState.Protected;
        currentAction = ActionState.Idle;

        fightActive = true;
        affectedByScroll = true;

        if (musicSync != null) musicSync.PlayAll();
    }

    protected override void AliveUpdate(float delta)
    {
        if (fightActive)
        {
            movingBasePos += GameManager.scrollSpeedVelocity * delta;
        }

        if (actionTimer > 0) actionTimer -= delta;

        HandleActionState(delta);

        base.AliveUpdate(delta);

        if (fightActive && currentAction == ActionState.Idle)
        {
            GlobalPosition = Utils.ClampToScreen(GlobalPosition, SCREEN_MARGIN, SCREEN_MARGIN);
        }
    }

    private void HandleActionState(float delta)
    {
        switch (currentAction)
        {
            case ActionState.Idle:
                velocity.Y = angleY;
                if (fightActive)
                {
                    Vector2 targetPos = new Vector2(movingBasePos.X, GlobalPosition.Y);
                    GlobalPosition = GlobalPosition.Lerp(targetPos, delta * 3f);


                    head.Rotation = Mathf.LerpAngle(head.Rotation, 0, delta * 8f);
                    if (Mathf.Abs(head.Rotation) < 0.1f && spriteHead != null) spriteHead.FlipV = false;

                    if (shell != null && (visualTween == null || !visualTween.IsRunning()))
                    {
                        shell.Scale = shell.Scale.Lerp(Vector2.One, delta * 10f);
                    }
                }
                break;

            case ActionState.PreDash:
                velocity = -dashDirection * dashRecoilSpeed;
                if (actionTimer <= 0) TriggerDash();
                break;

            case ActionState.Dashing:
                velocity = dashDirection * dashSpeed;
                if (actionTimer <= 0) StartReturn();
                break;

            case ActionState.Returning:
                Vector2 vectorToBase = movingBasePos - GlobalPosition;
                float dynamicReturnSpeed = returnSpeed + (GameManager.GetInstance().scrollSpeed * 0.005f);
                velocity = vectorToBase.Normalized() * dynamicReturnSpeed;

                if (GlobalPosition.DistanceTo(movingBasePos) < 20f) EndReturn();
                break;
        }
    }

    private void OnMusicBeat(TickEvent.BeatTypeBoss type)
    {
        if (!fightActive || isDeath || currentAction == ActionState.Dashing) return;

        switch (type)
        {
            case TickEvent.BeatTypeBoss.Shoot:
                if (currentAction == ActionState.Idle) TryFireOneSpike();
                break;
            case TickEvent.BeatTypeBoss.Reload:
                if (currentAction == ActionState.Idle) SpawnSpikes();
                break;
            case TickEvent.BeatTypeBoss.Dash:
                StartDashSequence();
                break;
            case TickEvent.BeatTypeBoss.Move:
                if (currentAction == ActionState.Idle) DoGrooveAnimation();
                break;
        }
    }

    private void StartDashSequence()
    {
        KillActiveTweens();
        currentAction = ActionState.PreDash;
        actionTimer = dashRecoilDuration;

        affectedByScroll = false;
        facePlayer = false;
        shellProjectileProtect = true;
        wing.SetSpeedDouble(true);
        currentState = EnemyState.Protected;

        ApplyTransformInstant(EnemyState.Protected);

        Vector2 playerPos = Player.GetInstance().GlobalPosition;
        dashDirection = Utils.DirectionTo(GlobalPosition, playerPos);
        bool isLookingLeft = dashDirection.X < 0;

        Flip(isLookingLeft);
        head.GlobalRotationDegrees = Utils.LookAt(playerPos, head.GlobalPosition);
        if (spriteHead != null) spriteHead.FlipV = isLookingLeft;

        visualTween = CreateTween();
        visualTween.TweenProperty(shell, "scale", new Vector2(1.15f, 0.85f), dashRecoilDuration)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
    }

    private void TriggerDash()
    {
        KillActiveTweens();
        currentAction = ActionState.Dashing;
        actionTimer = dashDuration;

        SwitchState(EnemyState.Outside, false);
        shell.Scale = Vector2.One;

        float tiltAngle = (dashDirection.X < 0) ? 25f : -25f;
        CreateTween().TweenProperty(shell, "rotation_degrees", tiltAngle, 0.1f);
    }

    private void StartReturn()
    {
        KillActiveTweens();
        currentAction = ActionState.Returning;
        SwitchState(EnemyState.Protected, false);
        CreateTween().TweenProperty(shell, "rotation_degrees", shellRetractedRot, 0.2f);
    }

    private void EndReturn()
    {
        KillActiveTweens();
        currentAction = ActionState.Idle;
        velocity = Vector2.Zero;
        GlobalPosition = movingBasePos;

        affectedByScroll = true;
        wing.SetSpeedDouble(false);
        shell.Scale = Vector2.One;
        shell.Rotation = 0;

        if (Player.GetInstance() != null)
        {
            Flip(Player.GetInstance().GlobalPosition.X < GlobalPosition.X);
            head.Rotation = 0;
        }
        ApplyTransformInstant(EnemyState.Protected);
    }

    private void SpawnSpikes()
    {
        if (spikeContainer == null) return;

        int activeSpikes = 0;
        foreach (var s in spikesList) if (s != null && !s.isActive) activeSpikes++;
        if (activeSpikes >= numSpikes) return;

        int radius = 155;
        float startAngle = -Mathf.Pi / 6;
        float endAngle = 5 * -Mathf.Pi / 6;
        float angleStep = (endAngle - startAngle) / (numSpikes - 1);

        for (int i = 0; i < numSpikes; i++)
        {
            if (i < spikesList.Count && spikesList[i] != null && !spikesList[i].isActive) continue;

            float angle = startAngle + (i * angleStep);
            Vector2 localPos = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);

            Spike spike = Spike.Create(spikeScene, Player.GetInstance(), localPos);
            spike.Rotation = angle;

            if (i < spikesList.Count) spikesList[i] = spike;
            else spikesList.Add(spike);

            spikeContainer.AddChild(spike);
            spike.lifeTimer.Stop();

            CreateTween().TweenProperty(spike, "position", localPos * spikeAdvanceMultiplier, shotPreDelay)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.InOut);
        }
    }

    private void TryFireOneSpike()
    {
        if (!fightActive || spikesList.Count == 0) return;

        List<int> availableIndexes = new List<int>();
        for (int i = 0; i < spikesList.Count; i++)
            if (spikesList[i] != null && !spikesList[i].isActive) availableIndexes.Add(i);

        if (availableIndexes.Count == 0) return;

        int randomIndex = availableIndexes[GameManager.rand.RandiRange(0, availableIndexes.Count - 1)];
        FireSpike(spikesList[randomIndex]);
    }

    private void FireSpike(Spike spike)
    {
        if (isDeath) return;
        if (fireTween != null && fireTween.IsRunning()) fireTween.Kill();

        fireTween = CreateTween();
        fireTween.TweenProperty(spike.sprite, "self_modulate", Colors.Red, shotPreDelay)
                 .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.InOut);

        fireTween.Finished += () => LaunchSpike(spike);
    }

    private void LaunchSpike(Spike spike)
    {
        if (spike.isActive) return;

        spike.isActive = true;

        Vector2 globalPos = spike.GlobalPosition;
        float globalRot = spike.GlobalRotation;

        spikeContainer.RemoveChild(spike);
        GameManager.GetInstance().gameContainer.AddChild(spike);

        spike.GlobalPosition = globalPos;
        spike.GlobalRotation = globalRot;

        spike.lifeTimer.Start();
        spike.particles.Emitting = true;
        spike.collision.Disabled = false;

        Projectiles.enemyProjectilesList.Add(spike);

        CreateTween().TweenProperty(spike, "scale", spike.Scale * spikeSizeMultiplier, 0.5f);
    }

    public void SwitchState(EnemyState newState, bool instant = false)
    {
        if (currentState == newState && !instant && visualTween == null) return;

        currentState = newState;
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
        shell.Scale = Vector2.One;
    }

    private void AnimateParts(EnemyState state)
    {
        KillActiveTweens();
        bool outside = (state == EnemyState.Outside);

        visualTween = CreateTween().SetParallel(true);

        visualTween.TweenProperty(head, "position", outside ? headOutsidePos : headRetractedPos, transitionDuration)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        visualTween.TweenProperty(head, "scale", outside ? headOutsideScale : headRetractedScale, transitionDuration)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);

        visualTween.TweenProperty(shell, "position", outside ? shellOutsidePos : shellRetractedPos, transitionDuration)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        visualTween.TweenProperty(shell, "rotation_degrees", outside ? shellOutsideRot : shellRetractedRot, transitionDuration)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
    }

    private void DoGrooveAnimation()
    {
        if (currentAction != ActionState.Idle || isDeath) return;
        KillActiveTweens();

        if (shell != null) shell.Scale = Vector2.One;

        beatCounter++;
        float direction = (beatCounter % 2 == 0) ? 1f : -1f;

        visualTween = CreateTween().SetParallel(true);

        visualTween.TweenProperty(shell, "position:y", shellRetractedPos.Y - grooveJumpHeight, 0.15f)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        visualTween.TweenProperty(shell, "rotation_degrees", shellRetractedRot + (grooveTiltAngle * direction), 0.15f)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);

        visualTween.Chain().TweenProperty(shell, "scale", new Vector2(0.95f, 1.05f), 0.05f);
        visualTween.Chain().TweenProperty(shell, "scale", Vector2.One, 0.2f)
            .SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);

        visualTween.Parallel().TweenProperty(shell, "position:y", shellRetractedPos.Y, 0.15f)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        visualTween.Parallel().TweenProperty(shell, "rotation_degrees", shellRetractedRot, 0.2f)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
    }

    private void KillActiveTweens()
    {
        if (visualTween != null && visualTween.IsRunning()) visualTween.Kill();
    }

    public override void EntityHit(int damage)
    {
        base.EntityHit(damage);
        KillActiveTweens();

        HUD.GetInstance().UpdateBossHealth(health);
    }

    public void OnHeadCollide(Area2D area)
    {
        if (!fightActive) return;

        EntityHit(1);
        if (shell != null) shell.ShellDamageAnimation();
    }

    public override void EntityDeath()
    {
        base.EntityDeath();
        KillActiveTweens();
        if (fireTween != null) fireTween.Kill();

        head.CollisionLayer = 0;
        head.CollisionMask = 0;

        currentAction = ActionState.Idle;
    }
}