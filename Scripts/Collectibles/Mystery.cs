using Godot;
using System;

public partial class Mystery : Movable
{
    private enum ItemType
    {
        Flower,
        Pow,
    }

    private ItemType itemType;

    [Export] private PackedScene FlowerScene;
    [Export] private PackedScene PowScene;

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);
        velocity.Y = angleY;
    }

    private void Hit()
    {
        Tween lTween;

        lTween = Effect.BumpScale(this, baseScale);

        lTween.Finished += () =>
        {
            SpawnCollectible();
            QueueFree();
        };
    }

    private void SpawnCollectible()
    {
        switch (itemType)
        {
            case ItemType.Flower:
                FlowerCollectible.Create(FlowerScene, GlobalPosition);
                break;

            case ItemType.Pow:
                PowCollectible.Create(PowScene, GlobalPosition);
                break;
        }
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        Hit();
    }
}
