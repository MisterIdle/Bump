using Godot;
using System;

using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NCollectibles;

// CHECK

public partial class HeartCollectible : Collectibles
{

    // =======================================================================
    // INIT
    // ========================================================================

    public static HeartCollectible Create(PackedScene pScene, Vector2 pPos)
    {
        HeartCollectible lCollectible = (HeartCollectible)pScene.Instantiate();
        lCollectible.Position = pPos;

        GameManager.GetInstance().gameContainer.AddChild(lCollectible);
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
