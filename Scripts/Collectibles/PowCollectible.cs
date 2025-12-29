using Godot;
using System;

public partial class PowCollectible : Collectibles
{
    public static PowCollectible Create(PackedScene pScene, Vector2 pPos)
    {
        PowCollectible collectible = (PowCollectible)pScene.Instantiate();
        collectible.Position = pPos;

        GameManager.GetInstance().gameContainer.AddChild(collectible);
        collectible.Initiate();

        return collectible;
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);
        Player.GetInstance().AddPow();
    }
}
