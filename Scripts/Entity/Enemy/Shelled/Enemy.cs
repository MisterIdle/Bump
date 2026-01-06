using Godot;
using System.Collections.Generic;

using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NHUD;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NStats;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class Enemy : Entity
{
    [Export] protected int hammerScore = 1000;

    [Export] private int outOfBoundMargin = 500;

    private float cameraShakeIntensity = 10f;
    private float cameraShakeDuration = 1f;
    private float cameraShakeFrequency = 1f;
    private float cameraShakeRandomness = 2f;

    [Export] private float squashStretchDuration = 0.4f;

    private int bossDamage = 20;
    private int clearAll = 9999;

    private bool killedByHammer;

    protected float globalPlayerDistance;

    // ========================================================================
    // UPDATE
    // ========================================================================

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

        globalPlayerDistance = Utils.DistanceTo(this, Player.GetInstance());

        Vector2 lOutAxis = Utils.OutOfBoundAxes(Position, outOfBoundMargin, outOfBoundMargin);
        if (lOutAxis != Vector2.Zero && this is not KingKoops)
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

                lEnemy.EntityHit(lEnemy.clearAll);
            }
        }
    }

    // ========================================================================
    // ANIMATION
    // ========================================================================

    protected void Flip(bool pFaceLeft)
    {
        float lDirection = pFaceLeft ? 1 : -1;
        Scale = new Vector2(Mathf.Abs(baseScale.X) * lDirection, baseScale.Y);

        baseScale = Scale;

        Effect.SquashStretch(this, baseScale, true);
    }

    // ========================================================================
    // COLLIDE
    // ========================================================================

    public override void EntityDeath()
    {
        base.EntityDeath();

        GameStats.enemyKills++;

        GameManager.CreateHealthCollectible(GlobalPosition);

        if (killedByHammer)
            Particule.Create(popScene, GlobalPosition);

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

            Effect.ShakeCamera(cameraShakeIntensity, cameraShakeDuration, cameraShakeFrequency, cameraShakeRandomness);
            ScaleTime.SlowMotion();
            Effect.SquashStretch(this, baseScale, false, false, squashStretchDuration);
        }
        else
        {
            killedByHammer = false;
            EntityHit();
        }
    }
}
