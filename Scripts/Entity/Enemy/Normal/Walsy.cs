using Godot;

public partial class Walsy : Enemy
{
    private AnimatedSprite2D animatedSprite;

    [Export] private NodePath animatedSpritePath;
    [Export] private float rotationSpeed;
    [Export] private float inverseRotationAmount = 15f;
    [Export] private float inverseRotationDuration = 0.2f;

    public override void Initiate()
    {
        base.Initiate();

        animatedSprite = (AnimatedSprite2D)GetNode(animatedSpritePath);

        string lAnim = animatedSprite.Animation;
        health = animatedSprite.SpriteFrames.GetFrameCount(lAnim);
    }

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);
        RotationDegrees += rotationSpeed * pDelta;
    }

    public override void EntityHit(int pDamage)
    {
        base.EntityHit(pDamage);

        animatedSprite.Frame++;

        if (health <= 0)
            QueueFree();

        Effect.BumpScale(this, baseScale);
        InverseRotation();
    }

    private void InverseRotation()
    {
        Tween tween = CreateTween();
        tween.TweenProperty(this, (string)PropertyName.RotationDegrees, RotationDegrees - inverseRotationAmount, inverseRotationDuration);
    }
}
