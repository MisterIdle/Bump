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
    private bool haveAbandoned;

    private const float MAX_BEHIND_TIME = 10f;

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
                if (globalPlayerDistance <= playerDistance)
                {
                    chaseState = ChaseState.Approaching;
                    Effect.BumpScale(this, baseScale);
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
                if (!haveAbandoned)
                    Abandonned();
                break;
        }

        velocity.Y = angleX;
    }

    private void ApproachPlayer(float delta)
    {
        velocity = playerDirection;

        if (globalPlayerDistance <= approchDistance)
        {
            chaseState = ChaseState.Chasing;
            shotTimer.Start();
            Effect.BumpScale(this, baseScale);
            wing.SetSpeedDouble(true);
        }
    }

    private void ChasePlayer()
    {
        Vector2 lTargetDirection;

        if (globalPlayerDistance <= chaseDistance)
            lTargetDirection = -playerDirection;
        else
            lTargetDirection = playerDirection;

        velocity = lTargetDirection;

        GD.Print(behindTimer.TimeLeft);

        if (!faceLeft)
        {
            if (behindTimer.IsStopped())
                behindTimer.Start();
        }
        else
            behindTimer.Stop();

        GlobalPosition = Utils.ClampToScreen(GlobalPosition, MARGIN, MARGIN);
    }

    private void BehindStop()
    {
        wing.SetSpeedDouble(false);
        chaseState = ChaseState.Abandoned;
        shotTimer.Stop();
    }

    private void Abandonned()
    {
        haveAbandoned = true;
        facePlayer = false;
        Flip(true);
        velocity = Vector2.Left;
        velocity.Y = angleY;
    }

    private void SpawnSpikes()
    {
        int lRadius = 25;
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
