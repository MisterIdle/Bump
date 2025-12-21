using Godot;
using System;

public partial class Walsy : Enemy
{
    [Export] private AnimatedSprite2D sprite;
    [Export] private float rotationSpeed;

    public override void Initiate()
    {
        base.Initiate();

        string lAnim = sprite.Animation;
        health = sprite.SpriteFrames.GetFrameCount(lAnim);

        GD.Print(health);
    }

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);
        RotationDegrees += rotationSpeed * pDelta;
    }

    protected override void TakeDamage()
    {
        base.TakeDamage();
        sprite.Frame++;
    }
}
