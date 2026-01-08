using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NToolsBox;
using Godot;
using System.Collections.Generic;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;

public partial class Projectiles : Movable
{
    [Export] private float lifeTime = 10;
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

        lifeTimer = new Timer();
        lifeTimer.WaitTime = lifeTime;
        lifeTimer.Autostart = true;
        lifeTimer.Timeout += Destory;
        AddChild(lifeTimer);
    }

    public static void ClearAll()
    {
        GameManager lGameManager = GameManager.GetInstance();

        if (lGameManager == null || !IsInstanceValid(lGameManager)) return;

        foreach (Projectiles lProjectiles in lGameManager.projectilesContainer.GetChildren())
        {
            if (IsInstanceValid(lProjectiles) && !lProjectiles.IsQueuedForDeletion())
            {
                lProjectiles.QueueFree();
            }
        }
    }

    public void Destory()
    {
        if (!IsInstanceValid(this) || IsQueuedForDeletion()) return;

        CallDeferred(MethodName.QueueFree);
    }

    protected override void Dispose(bool disposing)
    {
        Particule.Create(popScene, GlobalPosition);
    }
}