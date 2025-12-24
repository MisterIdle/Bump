using Godot;
using System.Collections.Generic;

public partial class Sykoops : ShelledEnemy
{
    private enum ChaseState
    {
        Patrolling,
        Approaching,
        Chasing,
        Abandoned
    }

    private ChaseState chaseState = ChaseState.Patrolling;

    [ExportGroup("Spike Settings")]
    [Export] private int numSpikes = 5;
    [Export] private PackedScene spikeScene;
    [Export] private NodePath spikeContainerPath;

    private Node2D spikeContainer;

    [ExportGroup("Shooting Settings")]
    private Timer shotTimer;
    private Tween fireTween;
    private Timer behindTimer;

    [Export] private int chaseDistance = 500;
    [Export] private int approchDistance = 200;
    [Export] private float fireRate = 3.5f;
    [Export] private float shotPreDelay = 0.5f;
    [Export] private float spikeAdvanceMultiplier = 1.3f;
    [Export] public float sizeMultiplier = 1.2f;

    private const float MAX_BEHIND_TIME = 10f;

    private const float LERP_SPEED = 0.05f;
    private const float MARGIN = -75f;

    private List<Spike> spikesList = new List<Spike>();
    private int lastShotIndex = -1;

    public override void Initiate()
    {
        base.Initiate();

        spikeContainer = (Node2D)GetNode(spikeContainerPath);

        shotTimer = new Timer();

        shotTimer.WaitTime = fireRate;
        shotTimer.Timeout += ActivateRandomSpike;
        shotTimer.Autostart = false;

        AddChild(shotTimer);

        behindTimer = new Timer();

        behindTimer.WaitTime = MAX_BEHIND_TIME;
        behindTimer.OneShot = true;
        behindTimer.Timeout += BehindStop;

        AddChild(behindTimer);

        shell.SetShellColor(Colors.Black);
    }

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        switch (chaseState)
        {
            case ChaseState.Patrolling:
                velocity.Y = angleY;
                if (globalPlayerDistance <= playerDistance)
                {
                    chaseState = ChaseState.Approaching;
                    Effect.BumpScale(this);
                }
                break;

            case ChaseState.Approaching:
                ApproachPlayer(pDelta);
                shotTimer.Start();
                break;

            case ChaseState.Chasing:
                ChasePlayer();
                break;

            case ChaseState.Abandoned:
                velocity = Vector2.Left;
                velocity.Y = angleY;
                break;
        }
    }

    private void ApproachPlayer(float delta)
    {
        Vector2 lPlayerPos = Player.GetInstance().GlobalPosition;
        Vector2 lTargetDirection = Utils.DirectionTo(GlobalPosition, lPlayerPos);

        velocity = velocity.Lerp(lTargetDirection, LERP_SPEED);

        if (globalPlayerDistance <= approchDistance)
        {
            chaseState = ChaseState.Chasing;
            shotTimer.Start();
            Effect.BumpScale(this);
        }
    }

    private void ChasePlayer()
    {
        Vector2 lPlayerPos = Player.GetInstance().GlobalPosition;
        Vector2 lDirectionToPlayer = Utils.DirectionTo(GlobalPosition, lPlayerPos);
        Vector2 lTargetDirection;

        bool lPlayerLeft = lPlayerPos.X < GlobalPosition.X;
        Scale = new Vector2(lPlayerLeft ? 1 : -1, 1);

        if (globalPlayerDistance <= chaseDistance)
            lTargetDirection = -lDirectionToPlayer;
        else
            lTargetDirection = lDirectionToPlayer;

        if (!lPlayerLeft)
        {
            if (behindTimer.IsStopped())
                behindTimer.Start();
        }
        else
            behindTimer.Stop();

        velocity = velocity.Lerp(lTargetDirection, LERP_SPEED);
        GlobalPosition = Utils.ClampToScreen(GlobalPosition, MARGIN, MARGIN);
    }

    private void BehindStop()
    {
        chaseState = ChaseState.Abandoned;
        shotTimer.Stop();
        Effect.BumpScale(this);
    }

    private void SpawnSpikes()
    {
        int lRadius = 50;
        float lStartAngle = -Mathf.Pi / 6;
        float lEndAngle = 5 * -Mathf.Pi / 6;
        float lAngleStep = (lEndAngle - lStartAngle) / (numSpikes - 1);

        for (int i = 0; i < numSpikes; i++)
        {
            if (i < spikesList.Count && spikesList[i] != null && !spikesList[i].isActive)
                continue;

            float lAngle = lStartAngle + i * lAngleStep;
            Vector2 lLocalPosition = new Vector2(Mathf.Cos(lAngle) * lRadius, Mathf.Sin(lAngle) * lRadius);

            Spike lSpike = Spike.Create(spikeScene, Player.GetInstance(), lLocalPosition);
            lSpike.Rotation = lAngle;

            if (i < spikesList.Count)
                spikesList[i] = lSpike;
            else
                spikesList.Add(lSpike);

            spikeContainer.AddChild(lSpike);
            lSpike.lifeTimer.Stop();

            Tween lTween = CreateTween();
            lTween.TweenProperty(lSpike, (string)PropertyName.Position, lLocalPosition * spikeAdvanceMultiplier, shotPreDelay)
                  .SetTrans(Tween.TransitionType.Back)
                  .SetEase(Tween.EaseType.InOut);
        }
    }

    private void ActivateRandomSpike()
    {
        int lIndex;

        if (spikesList.Count < numSpikes)
            SpawnSpikes();

        do
        {
            lIndex = GameManager.rand.RandiRange(0, spikesList.Count - 1);
        }
        while (lIndex == lastShotIndex || spikesList[lIndex] == null || spikesList[lIndex].isActive);

        lastShotIndex = lIndex;

        FireSpike(spikesList[lIndex]);
    }

    private void FireSpike(Spike pSpike)
    {
        if (isDeath) return;

        shotTimer.WaitTime = fireRate;

        fireTween = CreateTween();
        fireTween.TweenProperty(pSpike.sprite, (string)PropertyName.SelfModulate, Colors.Red, shotPreDelay)
              .SetTrans(Tween.TransitionType.Quad)
              .SetEase(Tween.EaseType.InOut);

        fireTween.Finished += () => LaunchSpike(pSpike);
    }

    private void LaunchSpike(Spike pSpike)
    {
        if (pSpike.isActive) return;

        pSpike.isActive = true;

        Vector2 lGlobalPosition = pSpike.GlobalPosition;
        float lGlobalRotation = pSpike.GlobalRotation;

        spikeContainer.RemoveChild(pSpike);
        GameManager.GetInstance().AddChild(pSpike);

        pSpike.GlobalPosition = lGlobalPosition;
        pSpike.GlobalRotation = lGlobalRotation;
        pSpike.lifeTimer.Start();

        Tween lTween = CreateTween();
        lTween.TweenProperty(pSpike, (string)PropertyName.Scale, pSpike.Scale * sizeMultiplier, 0.5f);

        Projectiles.enemyProjectilesList.Add(pSpike);

        pSpike.particles.Emitting = true;
        pSpike.collision.Disabled = false;

        SpawnSpikes();
    }

    public override void EntityHit(int pDamage)
    {
        base.EntityHit(pDamage);
        StartInvincible(invincibleTime);

        if (spikesList.Count < numSpikes) return;
        ActivateRandomSpike();
    }

    public override void EntityDeath()
    {
        base.EntityDeath();
        shotTimer.Stop();

        if (fireTween != null)
            fireTween.Kill();
    }
}
