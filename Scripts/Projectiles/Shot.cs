using Godot;

using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;

public partial class Shot : Projectiles
{
    [Export] private bool rotation;

    // ========================================================================
    // UPDATE
    // ========================================================================

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

        if (rotation)
            RotationDegrees += rotationSpeed * pDelta;
    }

    public static Shot Create(PackedScene pScene, Vector2 pPos, Vector2 pWay, float pAngle = 0)
    {
        Shot lShot = (Shot)pScene.Instantiate();
        
        GameManager.GetInstance().gameContainer.AddChild(lShot);
        lShot.GlobalPosition = pPos;

        lShot.Initiate();

        return lShot;
    }

    // ========================================================================
    // INIT
    // ========================================================================

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);
        QueueFree();
    }
}
