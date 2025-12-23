using Godot;
using System.Collections.Generic;

public partial class Enemy : Entity
{
    [Export] protected bool oneShotByHammer;

    public static List<Enemy> enemyList = new List<Enemy>();

    private const int MARGIN = 150;

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

        Vector2 lOut = Utils.OutOfBoundAxes(Position, MARGIN, MARGIN);

        if (lOut != Vector2.Zero) 
        {
            health = 0;
            QueueFree();
        }
    }

    public override void Initiate()
    {
        base.Initiate();

        enemyList.Add(this);
        velocity = Vector2.Left;
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        if (pArea.Owner is Hammer && oneShotByHammer)
            health = 0;

        EntityHit();
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

}
