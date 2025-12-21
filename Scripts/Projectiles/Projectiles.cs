using Godot;
using System;

public partial class Projectiles : Movable
{
    [Export] private int lifeTime = 5;

    [Export] protected float rotationSpeed;

    public Timer lifeTimer;

    public override void Initiate()
    {  
        base.Initiate();
        lifeTimer = new Timer();

        lifeTimer.WaitTime = lifeTime;
        lifeTimer.Autostart = true;
        lifeTimer.Timeout += QueueFree;

        AddChild(lifeTimer);
    }

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);
    }
}
