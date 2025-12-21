using Godot;
using System;

public partial class Shot : Projectiles
{
    [Export] private bool rotation;

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

        if (rotation)
            RotationDegrees += rotationSpeed * pDelta;
    }

    public static Shot Create(PackedScene pScene, Vector2 pPos, Vector2 pWay, float pAngle = 0)
    {
        Shot lShot = (Shot)pScene.Instantiate();

        lShot.Position = pPos;

        lShot.SetInitialMovement(pAngle, pWay);
        lShot.Initiate();

        return lShot;
    }

    private void SetInitialMovement(float pAngle, Vector2 pWay)
    {
        float lSpreadAngle = GameManager.rand.RandfRange(-pAngle, pAngle);
        float lFinalAngle = pWay.Angle() + lSpreadAngle;
        velocity = Vector2.FromAngle(lFinalAngle) * pWay.Length();
        Rotation = lFinalAngle;
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);
        QueueFree();
    }
}
