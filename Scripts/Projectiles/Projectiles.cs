using Godot;
using System.Collections.Generic;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;

public partial class Projectiles : Movable
{
    public static List<Projectiles> projectilesList = new List<Projectiles>();

    [Export] private float lifeTime = 5;
    [Export] protected PackedScene popScene;
    [Export] protected float rotationSpeed;
    public bool isActive;

    public Timer lifeTimer;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void Initiate()
    {
        base.Initiate();

        if (this is not Shot)
            projectilesList.Add(this);

        lifeTimer = new Timer();
        lifeTimer.WaitTime = lifeTime;
        lifeTimer.Autostart = true;
        lifeTimer.Timeout += Destory;
        AddChild(lifeTimer);
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

    public void Destory()
    {
        if (!IsInstanceValid(this)) return;

        if (IsQueuedForDeletion()) return;
        CallDeferred(MethodName.QueueFree);
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