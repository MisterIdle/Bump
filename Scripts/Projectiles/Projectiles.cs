using Godot;
using System;
using System.Collections.Generic;

public partial class Projectiles : Movable
{
    [Export] private float lifeTime = 5;

    [Export] protected PackedScene popScene;

    [Export] protected float rotationSpeed;

    public static List<Projectiles> enemyProjectilesList = new List<Projectiles>();

    public Timer lifeTimer;

    public override void Initiate()
    {  
        base.Initiate();
        lifeTimer = new Timer();

        lifeTimer.WaitTime = lifeTime;
        lifeTimer.Autostart = true;
        lifeTimer.Timeout += Destory;

        AddChild(lifeTimer);
    }

    public static List<Projectiles> GetEnemiesProjectilesOnScreen(Vector2 pPos)
    {
        List<Projectiles> lVisibleProjectiles = new List<Projectiles>();

        foreach (Projectiles lProjectiles in enemyProjectilesList)
        {
            if (lProjectiles.enable) lVisibleProjectiles.Add(lProjectiles);
        }

        return lVisibleProjectiles;
    }

    public virtual void Destory()
    {
        QueueFree();
    }

    protected override void Dispose(bool pDisposing)
    {
        if (enemyProjectilesList.Contains(this))
            enemyProjectilesList.Remove(this);

        base.Dispose(pDisposing);
    }

}
