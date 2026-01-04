using Godot;
using System;

public partial class Firebar : GameObject
{
    [Export] private float rotationSpeed = 45;

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);
        RotationDegrees += rotationSpeed * pDelta;
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        if (pArea is Entity lEntity)
            lEntity.EntityHit();

    }
}
