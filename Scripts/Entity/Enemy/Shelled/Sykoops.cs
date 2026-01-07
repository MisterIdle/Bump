using Godot;

using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

// Check

public partial class Sykoops : ShelledEnemy
{
    private enum ChaseState { Patrolling, Approaching, Chasing, Abandoned }
    private ChaseState chaseState = ChaseState.Patrolling;

    [ExportCategory("Logic")]
    [Export] private int chaseDistance = 500;
    [Export] private int approchDistance = 400;

    private Timer behindTimer;
    private bool haveAbandoned;

    private int playerDistanceMin = 600;

    private const float MAX_BEHIND_TIME = 10f;
    private const float MARGIN = -75f;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void Initiate()
    {
        base.Initiate();

        behindTimer = new Timer();

        behindTimer.WaitTime = MAX_BEHIND_TIME;
        behindTimer.OneShot = true;
        behindTimer.Timeout += BehindStop;

        AddChild(behindTimer);

        shell.SetShellColor(Colors.Black);
    }

    public static Sykoops Create(PackedScene pScene, Vector2 pPos)
    {
        Sykoops lSykoops = (Sykoops)pScene.Instantiate();
        lSykoops.GlobalPosition = pPos;
        return lSykoops;
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
                if (globalPlayerDistance <= playerDistanceMin)
                {
                    chaseState = ChaseState.Approaching;
                    Effect.BumpScale(this, baseScale);
                }
                break;

            case ChaseState.Approaching:
                velocity = playerDirection;
                StartSpike();

                if (globalPlayerDistance <= approchDistance)
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
                if (!haveAbandoned) 
                    Abandonned();
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

        GlobalPosition = Utils.ClampToScreen(GlobalPosition, MARGIN, MARGIN);
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

        velocity = Vector2.Left;
        velocity.Y = angleY;
    }

    // ========================================================================
    // COLLIDE
    // ========================================================================

    public override void EntityHit(int pDamage)
    {
        base.EntityHit(pDamage);
        StartInvincible(invincibleTime);
    }
}
