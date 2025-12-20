using Godot;
using System;

public partial class Shot : Projectiles
{
    public static Shot Create(PackedScene pScene, Vector2 pPos, Vector2 pWay, float pAngle = 0)
    {
        Shot lBullet = (Shot)pScene.Instantiate();

        lBullet.Position = pPos;

        lBullet.SetInitialMovement(pAngle, pWay);
        lBullet.Initiate();

        return lBullet;
    }

    private void SetInitialMovement(float pAngle, Vector2 pWay)
    {
        float lSpreadAngle = GameManager.GetInstance().rand.RandfRange(-pAngle, pAngle);
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
