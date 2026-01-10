using Godot;

using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class Sykoops : ShelledEnemy
{
    [ExportCategory("Phases")]
    [Export] private int chaseDistance = 500;
    [Export] private int approachDistance = 300;
    [Export] private int patrolRange = 600;

    [Export] private float maxBehindTime = 10f;

    private Timer behindTimer;
    private bool haveAbandoned;
    private enum ChaseState { Patrolling, Approaching, Chasing, Abandoned }
    private ChaseState chaseState = ChaseState.Patrolling;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void Initiate()
    {
        useSpikes = true;

        base.Initiate();

        behindTimer = new Timer();
        behindTimer.WaitTime = maxBehindTime;
        behindTimer.OneShot = true;
        behindTimer.Timeout += BehindStop;
        AddChild(behindTimer);

        shell.SetShellColor(Colors.Black);
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        switch (chaseState)
        {
            case ChaseState.Patrolling:
                if (globalPlayerDistance <= patrolRange)
                {
                    chaseState = ChaseState.Approaching;
                    Effect.BumpScale(this, baseScale);
                }
                break;

            case ChaseState.Approaching:
                velocity = playerDirection;
                StartSpike();
                if (globalPlayerDistance <= approachDistance)
                {
                    chaseState = ChaseState.Chasing;
                    Effect.BumpScale(this, baseScale);
                    wing.SetSpeedDouble(true);
                }
                break;

            case ChaseState.Chasing:
                ChasePlayer();
                break;

            case ChaseState.Abandoned:
                if (!haveAbandoned) Abandonned();
                break;
        }

        velocity.Y = angleX;
    }

    // ========================================================================
    // ATTACK
    // ========================================================================

    private void ChasePlayer()
    {
        Vector2 lTargetDirection = (globalPlayerDistance <= chaseDistance) ? -playerDirection : playerDirection;
        velocity = lTargetDirection;

        if (!faceLeft)
            if (behindTimer.IsStopped()) behindTimer.Start();
        else
            behindTimer.Stop();
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
}
