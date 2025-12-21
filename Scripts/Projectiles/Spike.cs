using Godot;
using System;

public partial class Spike : Projectiles
{
    private Entity target;
    public bool isActive;

    public static Spike Create(PackedScene pScene, Entity pTarget, Vector2 pPos)
    {
        Spike lSpike = (Spike)pScene.Instantiate();

        lSpike.Position = pPos;
        lSpike.target = pTarget;

        lSpike.Initiate();

        return lSpike;
    }

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

        if (!isActive) return;

        Vector2 lDirection = (target.Position - Position).Normalized();
        float lTargetAngle = lDirection.Angle();

        Rotation = Mathf.LerpAngle(Rotation, lTargetAngle, rotationSpeed * pDelta);
        velocity = Vector2.FromAngle(Rotation);
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        if (!isActive) return;
        QueueFree();
    }
}
