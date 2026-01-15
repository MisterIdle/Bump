using Godot;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class Sykoops : ShelledEnemy
{
    [ExportCategory("Phases")]
    [Export] private float spawnDuration = 1.0f;
    [Export] private int chaseDistance = 500;
    [Export] private float maxBehindTime = 10f;

    private Timer behindTimer;
    private Timer spawnTimer;
    private bool haveAbandoned;

    private enum ChaseState { Spawn, Chasing, Abandoned }
    private ChaseState chaseState = ChaseState.Spawn;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void Initiate()
    {
        useSpikes = true;

        base.Initiate();

        // Timer pour l'abandon (si derrière le joueur)
        behindTimer = new Timer();
        behindTimer.WaitTime = maxBehindTime;
        behindTimer.OneShot = true;
        behindTimer.Timeout += BehindStop;
        AddChild(behindTimer);

        // Timer pour le Spawn
        spawnTimer = new Timer();
        spawnTimer.WaitTime = spawnDuration;
        spawnTimer.OneShot = true;
        spawnTimer.Timeout += StartChasing;
        AddChild(spawnTimer);

        shell.SetShellColor(Colors.Black);

        // On commence en Spawn
        chaseState = ChaseState.Spawn;
        spawnTimer.Start();
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        switch (chaseState)
        {
            case ChaseState.Spawn:
                velocity.X = angleX;
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

    // ========================================================================
    // TRANSITIONS
    // ========================================================================

    private void StartChasing()
    {
        if (isDeath) return;

        affectedByScroll = true;
        chaseState = ChaseState.Chasing;
        StartSpike();
        wing.SetSpeedDouble(true);
        Effect.BumpScale(this, baseScale);
    }

    // ========================================================================
    // ATTACK
    // ========================================================================

    private void ChasePlayer()
    {
        Vector2 lTargetDirection = (globalPlayerDistance <= chaseDistance) ? -playerDirection : playerDirection;
        velocity = lTargetDirection;

        if (!faceLeft)
        {
            if (behindTimer.IsStopped()) behindTimer.Start();
        }
        else
        {
            behindTimer.Stop();
        }
    }

    // ========================================================================
    // ABANDONED
    // ========================================================================

    private void BehindStop()
    {
        wing.SetSpeedDouble(false);
        chaseState = ChaseState.Abandoned;
        StopSpike();
    }

    private void Abandonned()
    {
        haveAbandoned = true;
        facePlayer = false;
        Flip(true);
        direction = MoveDirection.Left;
        velocity.Y = angleY;
    }

    public override void EntityDeath()
    {
        spawnTimer?.Stop();
        behindTimer?.Stop();
        base.EntityDeath();
    }
}