using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NManager;
using Godot;

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

        lCollectible.Initiate();

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