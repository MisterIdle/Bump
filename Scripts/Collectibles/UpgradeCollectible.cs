using Godot;
using System;

public partial class UpgradeCollectible : Collectibles
{
    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);
        Player.GetInstance().LevelUp(true);
    }
}
