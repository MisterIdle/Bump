using Godot;
using System;

public partial class Coin : Collectibles
{
    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);
        //GameManager.GetInstance().AddCoin();
    }
}
