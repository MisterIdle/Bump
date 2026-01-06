using Godot;
using System;

using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NCollectibles;

public partial class Mystery : Movable
{
    [Export] private PackedScene FlowerScene;
    [Export] private PackedScene PowScene;

    private enum ItemType { Flower, Pow }
    [Export] private ItemType itemType;

    // ========================================================================
    // UPDATE
    // ========================================================================

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);
        velocity.Y = angleY;
    }

    // ========================================================================
    // COLLIDE
    // ========================================================================

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

    private void Hit()
    {
        Tween lHitTween;

        lHitTween = Effect.BumpScale(this, baseScale);

        lHitTween.Finished += () =>
        {
            SpawnCollectible();
            QueueFree();
        };
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        Hit();
    }
}
