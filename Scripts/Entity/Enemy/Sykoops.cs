using Godot;
using System.Collections.Generic;

public partial class Sykoops : ShelledEnemy
{
    private enum AttackState
    {
        Chase,
        Shoot
    }

    private AttackState lCurrentAttackState = AttackState.Chase;

    [ExportGroup("Spike Settings")]
    [Export] private int numSpikes = 5;
    [Export] private PackedScene spikeScene;
    [Export] private Node2D spikeContainer;

    [ExportGroup("Shooting Settings")]
    [Export] private Timer shotTimer;
    [Export] private int chaseDistance = 400;
    [Export] private float fireRate = 3.5f;
    [Export] private float shotPreDelay = 0.5f;
    [Export] private float spikeAdvanceMultiplier = 1.3f;

    private const float LERP_SPEED = 0.05f;

    private List<Spike> lSpikesList = new List<Spike>();
    private int lLastShotIndex = -1;

    public override void Initiate()
    {
        base.Initiate();

        shotTimer.WaitTime = fireRate;
        shotTimer.Timeout += ActivateRandomSpike;
        shotTimer.Autostart = true;
        shotTimer.Start();
    }

    protected override void Update(float delta)
    {
        base.Update(delta);

        if (lCurrentAttackState == AttackState.Chase)
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
            if (i < lSpikesList.Count && lSpikesList[i] != null && !lSpikesList[i].isActive)
                continue;

            float lAngle = lStartAngle + i * lAngleStep;
            Vector2 lLocalPosition = new Vector2(Mathf.Cos(lAngle) * lRadius, Mathf.Sin(lAngle) * lRadius);

            Spike lSpike = Spike.Create(spikeScene, Player.GetInstance(), lLocalPosition);
            lSpike.Rotation = lAngle;

            if (i < lSpikesList.Count)
                lSpikesList[i] = lSpike;
            else
                lSpikesList.Add(lSpike);

            spikeContainer.AddChild(lSpike);

            Tween lTween = CreateTween();
            lTween.TweenProperty(lSpike, Property.TWEEN_POSITION, lLocalPosition * spikeAdvanceMultiplier, shotPreDelay)
                  .SetTrans(Tween.TransitionType.Back)
                  .SetEase(Tween.EaseType.InOut);
        }
    }

    private void ActivateRandomSpike()
    {
        if (lSpikesList.Count < numSpikes)
            SpawnSpikes();

        int lIndex;

        do
        {
            lIndex = GameManager.rand.RandiRange(0, lSpikesList.Count - 1);
        }
        while (lIndex == lLastShotIndex || lSpikesList[lIndex] == null || lSpikesList[lIndex].isActive);

        lLastShotIndex = lIndex;

        FireSpike(lSpikesList[lIndex]);
    }

    private void FireSpike(Spike pSpike)
    {
        Tween lTween = CreateTween();
        lTween.TweenProperty(pSpike, Property.TWEEN_MODULATE, Colors.Red, shotPreDelay)
              .SetTrans(Tween.TransitionType.Quad)
              .SetEase(Tween.EaseType.InOut);

        lTween.Finished += () => LaunchSpike(pSpike);
    }

    private void LaunchSpike(Spike pSpike)
    {
        if (pSpike.isActive)
            return;

        Vector2 lGlobalPosition = pSpike.GlobalPosition;
        float lGlobalRotation = pSpike.GlobalRotation;

        Bump();

        if (pSpike.GetParent() != null)
            pSpike.GetParent().RemoveChild(pSpike);

        GameManager.GetInstance().AddChild(pSpike);

        pSpike.GlobalPosition = lGlobalPosition;
        pSpike.GlobalRotation = lGlobalRotation;
        pSpike.isActive = true;

        pSpike.CollisionLayer = 2;
        pSpike.CollisionMask = 1;

        SpawnSpikes();
    }
}
