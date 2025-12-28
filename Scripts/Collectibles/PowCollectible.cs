using Godot;
using System;

public partial class PowCollectible : Collectibles
{
    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);
        Player.GetInstance().AddPow();
    }
}
