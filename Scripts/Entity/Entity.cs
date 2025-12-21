using Godot;
using System;

public partial class Entity : Movable
{
    [Export] protected int health = 1;
    [Export] public bool invulnerability;

    [Export] protected PackedScene popScene;

    [Export] public bool isDeath;

    [Export] private float scaleBump = 1.2f;
    [Export] private float scaleDuration = 0.1f;

    private Tween bumpTween;
    private Vector2 baseScale;

    [Export] protected float fallDeath = 20f;
    [Export] protected float deathJump = 5f;

    public override void Initiate()
    {
        base.Initiate();
        baseScale = Scale;
    }

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

        if (isDeath)
        {
            velocity += Vector2.Down * speed * pDelta;
            RotationDegrees++;
            return;
        }
        else
            AliveUpdate(pDelta);
    }

    protected virtual void AliveUpdate(float pDelta) { }

    public virtual void EntityHit()
    {
        if (invulnerability) return;

        health--;

        if (health <= 0)
            EntityDeath();
    }

    protected virtual void EntityDeath()
    {
        velocity = Vector2.Zero;

        isDeath = true;
        invulnerability = true;

        Particule.Create(popScene, GlobalPosition);

        CollisionLayer = 0;
        CollisionMask = 0;

        speed = fallDeath;

        DeathBump();
    }

    private void DeathBump()
    {
        velocity.Y = -deathJump;
    }

    protected void Bump()
    {
        bumpTween?.Kill();

        bumpTween = CreateTween();

        bumpTween.TweenProperty(this, (string)PropertyName.Scale, baseScale * scaleBump, scaleDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        bumpTween.TweenProperty(this, (string)PropertyName.Scale, baseScale, scaleDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        if (isDeath) return;
    }
}
