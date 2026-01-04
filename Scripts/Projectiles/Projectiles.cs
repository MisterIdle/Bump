using Godot;
using System.Collections.Generic;

public partial class Projectiles : Movable
{
    // Utilisation d'une liste statique pour suivre tous les projectiles actifs
    public static List<Projectiles> projectilesList = new List<Projectiles>();

    [Export] private float lifeTime = 5;
    [Export] protected PackedScene popScene;
    [Export] protected float rotationSpeed;
    public bool isActive;

    public Timer lifeTimer;

    public override void Initiate()
    {
        base.Initiate();

        // On ne track pas les tirs simples (Shot) s'ils sont gérés ailleurs, 
        // sinon on ajoute tout projectile à la liste globale.
        if (this is not Shot)
            projectilesList.Add(this);

        lifeTimer = new Timer();
        lifeTimer.WaitTime = lifeTime;
        lifeTimer.Autostart = true;
        lifeTimer.Timeout += Destory;
        AddChild(lifeTimer);
    }

    public void Destory()
    {
        if (!IsInstanceValid(this)) return;

        if (IsQueuedForDeletion()) return;
        CallDeferred(MethodName.QueueFree);
    }

    public static void ClearAll()
    {
        var listCopy = new List<Projectiles>(projectilesList);

        foreach (var p in listCopy)
        {
            if (IsInstanceValid(p) && p.isActive)
            {
                p.Destory();
            }
        }

        projectilesList.Clear();
    }

    protected override void Dispose(bool pDisposing)
    {
        if (projectilesList != null && projectilesList.Contains(this))
        {
            projectilesList.Remove(this);
        }

        base.Dispose(pDisposing);
    }
}