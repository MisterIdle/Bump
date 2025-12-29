using Godot;
using System;

public partial class HeartCollectible : Collectibles
{
    public static HeartCollectible Create(PackedScene pScene, Vector2 pPos)
    {
        HeartCollectible collectible = (HeartCollectible)pScene.Instantiate();
        collectible.Position = pPos;

        GameManager.GetInstance().gameContainer.AddChild(collectible);
        collectible.Initiate();

        return collectible;
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);
        Player.GetInstance().AddHealth();
    }
}
