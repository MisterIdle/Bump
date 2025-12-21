using Godot;
using System;

public partial class Entity : Movable
{
    [Export] protected int health = 1;

    [Export] private float scaleBump = 1.2f;
    [Export] private float scaleDuration = 0.1f;

    protected virtual void TakeDamage()
    {
        health--;
        HitAnimation();

        if (health <= 0)
        {
            health = 0;
            Death();
        }
    }

    private void HitAnimation()
    {
        Tween lTween = CreateTween();

        lTween.TweenProperty(this, Property.TWEEN_SELF_MODULATE, Colors.Red, 0.1f);
        lTween.TweenProperty(this, Property.TWEEN_SELF_MODULATE, Colors.White, 0.1f);
    }

    protected void Bump()
    {
        Vector2 lBaseScale = Scale;
        Vector2 lBumpScale = lBaseScale * scaleBump;

        Tween lTween = CreateTween();

        lTween.TweenProperty(this, Property.TWEEN_SCALE, lBumpScale, scaleDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        lTween.TweenProperty(this, Property.TWEEN_SCALE, lBaseScale, scaleDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
    }

    protected void Death()
    {
        QueueFree();
    }

    protected override void OnCollide(Area2D pArea)
    {
        TakeDamage();
    }
}
