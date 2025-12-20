using Godot;
using System;

public partial class Projectiles : Movable
{
    [Export] private int lifeTime = 5;

    [Export] private bool rotation;
    [Export] private float rotationSpeed;

    public override void _Ready()
    {
        Timer lLifeTimer = new Timer();

        lLifeTimer.WaitTime = lifeTime;
        lLifeTimer.Autostart = true;
        lLifeTimer.Timeout += Destroy;

        AddChild(lLifeTimer);
    }

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

        if (rotation)
            RotationDegrees += rotationSpeed * pDelta;
    }

    private void Destroy()
    {
        QueueFree();
    }

    protected override void OnCollide(Area2D pArea)
    {
    }
}
