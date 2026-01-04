using Godot;
using System.Collections.Generic;

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

    private int bossClearDamage = 20;
    private int clearAllDamage = 9999;

    protected float globalPlayerDistance;

    public override void Initiate()
    {
        base.Initiate();
        enemyList.Add(this);
    }

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

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        bool killedByHammer = false;

        if (pArea.Owner is Hammer)
        {
            EntityHit(Hammer.GetInstance().attackForce);

            Camera.Shake(cameraShakeIntensity, cameraShakeDuration, cameraShakeFrequency, cameraShakeRandomness);

            Time.SlowMotion();
            Effect.SquashStretch(this, baseScale, false, false, squashStretchDuration);

            if (health <= 0)
                killedByHammer = true;
        }
        else
        {
            EntityHit();
        }

        if (health <= 0)
            EntityDeath(killedByHammer);
    }

    public void EntityDeath(bool pKilledByHammer = false)
    {
        base.EntityDeath();

        GameStats.enemyKills++;

        enemyList.Remove(this);

        GameManager.CreateHealthCollectible(GlobalPosition);

        if (pKilledByHammer)
            Particule.Create(popScene, GlobalPosition);

        int finalScore = pKilledByHammer ? score + hammerScore : score;
        HUD.GetInstance().CreateBubbleScore(Position, finalScore);
    }

    public static void ClearAll(bool pBossOneShot)
    {
        for (int i = enemyList.Count - 1; i >= 0; i--)
        {
            if (i >= enemyList.Count) continue;

            Enemy enemy = enemyList[i];

            if (IsInstanceValid(enemy))
            {
                if (enemy is KingKoops && !pBossOneShot)
                {
                    enemy.EntityHit(enemy.bossClearDamage);
                    return;
                }

                enemy.health = 0;
                enemy.EntityHit(enemy.clearAllDamage);
            }
        }
    }

    protected override void Dispose(bool pDisposing)
    {
        enemyList.Remove(this);
        base.Dispose(pDisposing);
    }
}
