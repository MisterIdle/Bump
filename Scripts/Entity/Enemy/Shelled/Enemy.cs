using Godot;
using System.Collections.Generic;

using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NHUD;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NStats;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

// CHECK

public partial class Enemy : Entity
{
    [Export] protected int hammerScore = 1000;

    [Export] private int outOfBoundMargin = 250;

    [Export] private float cameraShakeIntensity = 2f;
    [Export] private float cameraShakeDuration = 0.2f;
    [Export] private float cameraShakeFrequency = 1f;
    [Export] private float cameraShakeRandomness = 2f;

    [Export] private float squashStretchDuration = 0.4f;

    [Export] private float flipCooldownDuration = 0.5f;
    private Timer flipTimer;

    private int bossDamage = 20;
    private int clearAll = 9999;

    private bool killedByHammer;

    protected float globalPlayerDistance;

    public override void _Ready()
    {
        base._Ready();

        flipTimer = new Timer();
        flipTimer.OneShot = true;
        AddChild(flipTimer);
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

        globalPlayerDistance = Utils.DistanceTo(this, Player.GetInstance());

        if (Utils.IsOutOfBound(Position, outOfBoundMargin))
        {
            health = 0;
            QueueFree();
        }
    }

    // ========================================================================
    // STATIC CLEAR
    // ========================================================================

    public static void ClearAll(bool pBossOneShot, bool pTotalClear)
    {
        GameManager lGameManager = GameManager.GetInstance();

        if (lGameManager == null || !IsInstanceValid(lGameManager)) return;

        if (lGameManager.enemyContainer == null) return;

        foreach (Enemy lEnemy in lGameManager.enemyContainer.GetChildren())
        {
            if (IsInstanceValid(lEnemy) && lEnemy.enable)
            {
                if (pTotalClear)
                {
                    lEnemy.QueueFree();
                    continue;
                }

                if (lEnemy is KingKoops && !pBossOneShot)
                {
                    lEnemy.EntityHit(lEnemy.bossDamage);
                    continue;
                }

                lEnemy.health = 0;
                lEnemy.EntityHit(lEnemy.clearAll);
            }
        }
    }

    // ========================================================================
    // ANIMATION
    // ========================================================================

    protected void Flip(bool pFaceLeft)
    {
        if (!flipTimer.IsStopped()) return;

        float lCurrentDirection = Mathf.Sign(Scale.X);
        float lTargetDirection = pFaceLeft ? 1 : -1;
        if (lCurrentDirection == lTargetDirection) return;

        float lDirection = pFaceLeft ? 1 : -1;
        Scale = new Vector2(Mathf.Abs(baseScale.X) * lDirection, baseScale.Y);
        baseScale = Scale;

        Effect.SquashStretch(this, baseScale, true);

        flipTimer.Start(flipCooldownDuration);
    }

    // ========================================================================
    // COLLIDE
    // ========================================================================

    public override void EntityHit(int pDamage)
    {
        base.EntityHit(pDamage);
        StartInvincible(invincibleTime);
    }

    public override void EntityDeath()
    {
        base.EntityDeath();

        GameStats.enemyKills++;

        GameManager.CreateHealthCollectible(GlobalPosition);

        if (killedByHammer)
            Particule.Create(popScene, Position);

        int lFinalScore = killedByHammer ? score + hammerScore : score;

        HUD.GetInstance().CreateBubbleScore(Position, lFinalScore);
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        if (pArea.Owner is Hammer)
        {
            killedByHammer = true;
            EntityHit(Hammer.GetInstance().attackForce);

            Effect.Shake(cameraShakeIntensity, cameraShakeDuration, cameraShakeFrequency, cameraShakeRandomness);
            ScaleTime.SlowMotion();
            Effect.SquashStretch(this, baseScale, false, false, squashStretchDuration);
        }
        else
        {
            killedByHammer = false;
            EntityHit(1);
        }
    }
}
