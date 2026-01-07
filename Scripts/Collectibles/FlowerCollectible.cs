using Godot;
using System;

using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NCollectibles;

public partial class FlowerCollectible : Collectibles
{
    // ========================================================================
    // INIT
    // ========================================================================

    public static FlowerCollectible Create(PackedScene pScene, Vector2 pPos)
    {
        FlowerCollectible lCollectible = (FlowerCollectible)pScene.Instantiate();
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

        Player lPlayer = Player.GetInstance();
        lPlayer.LevelUp(true);
        Effect.SquashStretch(this, baseScale, true);
    }
}
