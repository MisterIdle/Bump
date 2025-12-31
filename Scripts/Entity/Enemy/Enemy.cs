using Godot;
using System.Collections.Generic;

public partial class Enemy : Entity
{
    [Export] protected int playerDistance = 600;
    [Export] protected int hammerScore = 1000;

    protected float globalPlayerDistance;
    private bool playerSeen = false;

    public static List<Enemy> enemyList = new List<Enemy>();

    private const int MARGIN = 350;

    public override void Initiate()
    {
        base.Initiate();

        enemyList.Add(this);
    }


    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

        globalPlayerDistance = Utils.DistanceTo(this, Player.GetInstance());

        if (globalPlayerDistance <= playerDistance && !playerSeen)
        {
            Effect.BumpScale(this, baseScale);
            playerSeen = true;
        }

        Vector2 lOut = Utils.OutOfBoundAxes(Position, MARGIN, MARGIN);
        if (lOut != Vector2.Zero)
        {
            health = 0;
            QueueFree();
        }
    }

    public static List<Enemy> GetEnemiesOnScreen(Vector2 pPos)
    {
        List<Enemy> lVisibleEnemies = new List<Enemy>();

        foreach (Enemy lEnemy in enemyList)
        {
            if (lEnemy.enable && !lEnemy.isDeath)
                lVisibleEnemies.Add(lEnemy);
        }

        for (int i = 0; i < lVisibleEnemies.Count - 1; i++)
        {
            for (int j = i + 1; j < lVisibleEnemies.Count; j++)
            {
                float lDistanceA = lVisibleEnemies[i].GlobalPosition.DistanceTo(pPos);
                float lDistanceB = lVisibleEnemies[j].GlobalPosition.DistanceTo(pPos);

                if (lDistanceA > lDistanceB)
                {
                    Enemy lTemp = lVisibleEnemies[i];
                    lVisibleEnemies[i] = lVisibleEnemies[j];
                    lVisibleEnemies[j] = lTemp;
                }
            }
        }

        return lVisibleEnemies;
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        bool killedByHammer = false;

        if (pArea.Owner is Hammer)
        {
            EntityHit(Hammer.GetInstance().attackForce);

            Camera.Shake(2, 0.2f, 1, 2);
            Time.SlowMotion();
            Effect.SquashStretch(this, baseScale, false, false, 0.4f);

            if (health <= 0)
                killedByHammer = true;
        }
        else
        {
            EntityHit();
        }

        if (health <= 0)
        {
            EntityDeath(killedByHammer);
        }
    }

    public void EntityDeath(bool pKilledByHammer = false)
    {
        base.EntityDeath();

        GameManager.CreateHealthCollectible(GlobalPosition);

        if (pKilledByHammer)
            Particule.Create(popScene, GlobalPosition);

        int lFinalScore = pKilledByHammer ? score + hammerScore : score;
        HUD.GetInstance().CreateBubbleScore(Position, lFinalScore);
    }


    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        enemyList.Remove(this);
    }

}
