using Godot;
using System;

using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity;

namespace Com.IsartDigital.Shmup.NGameObject;

// CHECK

public partial class Firebar : GameObject
{
    [Export] private float rotationSpeed = 45;

    // ========================================================================
    // UPDATE
    // ========================================================================

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);
        RotationDegrees += rotationSpeed * pDelta;
    }

    // ========================================================================
    // COLLIDE
    // ========================================================================

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        if (pArea is Entity lEntity)
            lEntity.EntityHit();

    }
}
