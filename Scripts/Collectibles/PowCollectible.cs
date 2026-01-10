using Godot;
using System;

using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NCollectibles;

// CHECK

public partial class PowCollectible : Collectibles
{
    // ========================================================================
    // INIT
    // ========================================================================

    public static PowCollectible Create(PackedScene pScene, Vector2 pPos)
    {
        PowCollectible lCollectible = (PowCollectible)pScene.Instantiate();

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
        Player.GetInstance().AddPow();
    }
}
