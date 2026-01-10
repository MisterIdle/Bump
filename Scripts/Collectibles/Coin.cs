using Godot;
using System;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NCollectibles;

public partial class Coin : Collectibles
{
    // ========================================================================
    // COLLIDE
    // ========================================================================

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);
        //GameManager.GetInstance().AddCoin();
    }
}
