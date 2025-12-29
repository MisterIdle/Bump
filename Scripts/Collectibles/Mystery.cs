using Godot;
using System;

public partial class Mystery : Movable
{
    private enum Type
    {
        Flower,
        Pow,
    }

    private Type type;

    [Export] private PackedScene FlowerScene;
    [Export] private PackedScene PowScene;

    private AnimatedSprite2D spriteAnimated;
    [Export] private NodePath spriteAnimatedPath;

    public override void Initiate()
    {
        base.Initiate();

        spriteAnimated = (AnimatedSprite2D)GetNode(spriteAnimatedPath);

        spriteAnimated.Play();
    }

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
        switch (type)
        {
            case Type.Flower:
                FlowerCollectible.Create(FlowerScene, GlobalPosition);
                break;
            case Type.Pow:
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
