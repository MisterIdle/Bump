using Godot;

using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class Walsy : Enemy
{
    private AnimatedSprite2D animatedSprite;

    [Export] private NodePath animatedSpritePath;
    [Export] private float rotationSpeed;
    [Export] private float inverseRotationAmount = 15f;
    [Export] private float inverseRotationDuration = 0.2f;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void Initiate()
    {
        base.Initiate();

        animatedSprite = (AnimatedSprite2D)GetNode(animatedSpritePath);

        string lAnim = animatedSprite.Animation;
        health = animatedSprite.SpriteFrames.GetFrameCount(lAnim);
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);
        RotationDegrees += rotationSpeed * pDelta;
    }

    // ========================================================================
    // ANIMATION
    // ========================================================================

    private void InverseRotation()
    {
        Tween lInverseTween = CreateTween();
        lInverseTween.TweenProperty(this, (string)PropertyName.RotationDegrees, RotationDegrees - inverseRotationAmount, inverseRotationDuration);
    }

    // ========================================================================
    // COLLIDE
    // ========================================================================

    public override void EntityHit(int pDamage = 1)
    {
        base.EntityHit(pDamage);

        animatedSprite.Frame++;

        Effect.BumpScale(this, baseScale);
        InverseRotation();
    }

    public override void EntityDeath()
    {
        base.EntityDeath();
        QueueFree();
    }
}
