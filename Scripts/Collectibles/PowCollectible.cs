using Godot;
using System;

public partial class PowCollectible : Collectibles
{
    public override void Initiate()
    {
        base.Initiate();
        StartParachuteEffect();
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);
        Player.GetInstance().AddPow();
    }
}
