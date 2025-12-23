using Godot;
using System;

public partial class Walsy : Enemy
{
    private AnimatedSprite2D animatedSprite;
    [Export] private NodePath animatedSpritePath;

    [Export] private float rotationSpeed;

    public override void Initiate()
    {
        base.Initiate();

        animatedSprite = (AnimatedSprite2D)GetNode(animatedSpritePath);

        string lAnim = animatedSprite.Animation;
        health = animatedSprite.SpriteFrames.GetFrameCount(lAnim);

        GD.Print(health);
    }

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);
        RotationDegrees += rotationSpeed * pDelta;
    }

    public override void EntityHit()
    {
        base.EntityHit();

        animatedSprite.Frame++;

        if (health <= 0)
            QueueFree();

        Effect.BumpScale(this);
        InverseRotation();
    }

    private void InverseRotation()
    {
        Tween lTween = CreateTween();

        lTween.TweenProperty(this, (string)PropertyName.RotationDegrees, RotationDegrees - 15, 0.2f);
    }
}
