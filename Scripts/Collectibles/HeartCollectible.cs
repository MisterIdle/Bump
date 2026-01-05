using Godot;
using System;

using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NCollectibles;

public partial class HeartCollectible : Collectibles
{

    // =======================================================================
    // INIT
    // ========================================================================

    public static HeartCollectible Create(PackedScene pScene, Vector2 pPos)
    {
        HeartCollectible collectible = (HeartCollectible)pScene.Instantiate();
        collectible.Position = pPos;

        GameManager.GetInstance().gameContainer.AddChild(collectible);
        collectible.Initiate();

        return collectible;
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
