using Godot;
using System.Collections.Generic;

using Com.IsartDigital.Shmup.NToolsBox;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NStats;
using Com.IsartDigital.Shmup.NHUD;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class Enemy : Entity
{
    public static List<Enemy> enemyList = new List<Enemy>();

    [Export] protected int hammerScore = 1000;

    [Export] private int outOfBoundMargin = 350;

    [Export] private float cameraShakeIntensity = 2f;
    [Export] private float cameraShakeDuration = 0.2f;
    [Export] private float cameraShakeFrequency = 1f;
    [Export] private float cameraShakeRandomness = 2f;

    [Export] private float squashStretchDuration = 0.4f;

    private int bossDamage = 20;
    private int clearAllDamage = 9999;

    private bool killedByHammer;

    protected float globalPlayerDistance;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void Initiate()
    {
        base.Initiate();
        enemyList.Add(this);
    }

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
        for (int i = enemyList.Count - 1; i >= 0; i--)
        {
            if (i >= enemyList.Count) continue;

            Enemy lEnemy = enemyList[i];

            if (IsInstanceValid(lEnemy))
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

                lEnemy.EntityHit(lEnemy.clearAllDamage);
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

        enemyList.Remove(this);

        GameManager.CreateHealthCollectible(GlobalPosition);

        if (killedByHammer)
        {
            Particule.Create(popScene, GlobalPosition);
            GD.Print("oui hammer 2");
        }

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
            EntityHit();
        }
    }

    protected override void Dispose(bool pDisposing)
    {
        enemyList.Remove(this);
        base.Dispose(pDisposing);
    }
}
