using Godot;
using System.Collections.Generic;

public partial class Sykoops : ShelledEnemy
{
    private enum AttackState
    {
        Chase,
        Shoot
    }

    private AttackState currentAttackState = AttackState.Chase;

    [ExportGroup("Spike Settings")]
    [Export] private int numSpikes = 5;
    [Export] private PackedScene spikeScene;
    [Export] private NodePath spikeContainerPath;

    private Node2D spikeContainer;

    [ExportGroup("Shooting Settings")]
    private Timer shotTimer;
    private Tween fireTween;
    [Export] private int chaseDistance = 400;
    [Export] private float fireRate = 3.5f;
    [Export] private float shotPreDelay = 0.5f;
    [Export] private float spikeAdvanceMultiplier = 1.3f;
    [Export] public float sizeMultiplier = 1.2f;

    private const float LERP_SPEED = 0.05f;

    private List<Spike> spikesList = new List<Spike>();
    private int lastShotIndex = -1;

    public override void Initiate()
    {
        base.Initiate();

        spikeContainer = (Node2D)GetNode(spikeContainerPath);

        shotTimer = new Timer();
        shotTimer.WaitTime = fireRate;
        shotTimer.Timeout += ActivateRandomSpike;
        shotTimer.Autostart = true;

        AddChild(shotTimer);
    }


    protected override void AliveUpdate(float delta)
    {
        base.AliveUpdate(delta);

        if (currentAttackState == AttackState.Chase)
            ChasePlayer();
    }

    private void ChasePlayer()
    {
        bool lIsPlayerLeft = Player.GetInstance().GlobalPosition.X < GlobalPosition.X;
        Scale = new Vector2(lIsPlayerLeft ? 1 : -1, 1);

        float lPlayerDistance = Look.DistanceTo(this, Player.GetInstance());
        Vector2 lPlayerDirection = Look.DirectionTo(this, Player.GetInstance());

        Vector2 lTargetDirection = (lPlayerDistance >= chaseDistance) ? lPlayerDirection : -lPlayerDirection;
        velocity = velocity.Lerp(lTargetDirection, LERP_SPEED);
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
        if (spikesList.Count < numSpikes)
            SpawnSpikes();

        int lIndex;

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

        pSpike.CollisionLayer = 2;
        pSpike.CollisionMask = 1;

        pSpike.particles.Emitting = true;

        SpawnSpikes();
    }

    public override void EntityHit()
    {
        base.EntityHit();

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
