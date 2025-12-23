using Godot;
using System;

public partial class Entity : Movable
{
    [Export] public int health = 1;

    [Export] protected PackedScene popScene;

    [Export] public bool isDeath;

    [Export] protected float frequency = 1.0f;
    [Export] protected float amplitude = 1.0f;

    [Export] protected float fallDeath = 20f;
    [Export] protected float deathJump = 5f;

    protected float elapsedTime;

    protected float angleX;
    protected float angleY;

    public override void Initiate()
    {
        base.Initiate();
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

    protected virtual void AliveUpdate(float pDelta) 
    {
        elapsedTime += pDelta;

        angleX = Mathf.Sin(elapsedTime * frequency) * amplitude;
        angleY = Mathf.Cos(elapsedTime * frequency) * amplitude;
    }

    public virtual void EntityHit()
    {
        health--;

        if (health <= 0)
            EntityDeath();
    }

    public virtual void EntityDeath()
    {
        velocity = Vector2.Zero;

        isDeath = true;

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

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        if (isDeath) return;
    }
}
