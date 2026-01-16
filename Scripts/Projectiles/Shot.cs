using Godot;
using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;

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
        if (!IsInstanceValid(GameManager.GetInstance()))
            return null;

        var gm = GameManager.GetInstance();

        if (!IsInstanceValid(gm.gameContainer))
            return null;

        Shot shot = pScene.Instantiate<Shot>();

        gm.gameContainer.AddChild(shot);

        shot.GlobalPosition = pPos;
        shot.GlobalRotation = pWay.Angle();

        shot.Initiate();

        return shot;
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);
        QueueFree();
    }
}