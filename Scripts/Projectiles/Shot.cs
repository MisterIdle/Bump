using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NStats;
using Godot;

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

        GameManager lGameManager = GameManager.GetInstance();

        if (!IsInstanceValid(lGameManager.gameContainer))
            return null;

        Shot lShot = pScene.Instantiate<Shot>();

        lGameManager.gameContainer.AddChild(lShot);

        lShot.GlobalPosition = pPos;
        lShot.GlobalRotation = pWay.Angle();

        lShot.Initiate();

        GameStats.shotsFired++;

        return lShot;
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);
        QueueFree();
    }
}