using Godot;

using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NCollectibles;

public partial class HeartCollectible : Collectibles
{

    // =======================================================================
    // INIT
    // ========================================================================

    public static HeartCollectible Create(PackedScene pScene, Vector2 pPos)
    {
        HeartCollectible lCollectible = (HeartCollectible)pScene.Instantiate();

        GameManager.GetInstance().gameContainer.AddChild(lCollectible);
        lCollectible.GlobalPosition = pPos;

        return lCollectible;
    }

    // ========================================================================
    // COLLIDE
    // ========================================================================

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);
        Player.GetInstance().AddHealth();
    }
}