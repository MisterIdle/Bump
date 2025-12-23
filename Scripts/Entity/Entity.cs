using Godot;
using System;

public partial class Entity : Movable
{
    [Export] public int health = 1;

    [Export] protected PackedScene popScene;

    public bool invincible;
    [Export] protected float invincibleTime = 1f;
    [Export] public bool isDeath;

    [Export] protected float frequency = 1.0f;
    [Export] protected float amplitude = 1.0f;

    [Export] protected float fallDeath = 20f;
    [Export] protected float deathJump = 5f;

    [Export] private float deathRotationSpeed = 60f;
    [Export] private float deathGravity = 20f;

    protected Timer invincibleTimer;

    protected float elapsedTime;

    protected float angleX;
    protected float angleY;

    public override void Initiate()
    {
        base.Initiate();

        invincibleTimer = new Timer();

        invincibleTimer.OneShot = true;
        invincibleTimer.Timeout += EndInvincible;

        AddChild(invincibleTimer);
    }

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

        if (isDeath)
        {
            velocity += Vector2.Down * deathGravity * pDelta;
            RotationDegrees += deathRotationSpeed * pDelta;
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
        if (invincible || isDeath) return;

        GD.Print(Name + " Hit");

        health--;

        if (health <= 0)
            EntityDeath();
    }

    public virtual void EntityDeath()
    {
        velocity = Vector2.Zero;

        isDeath = true;

        Particule.Create(popScene, GlobalPosition);

        CollisionMask = 0;

        speed = fallDeath;

        DeathBump();
    }

    protected virtual void StartInvincible(float pDuration)
    {
        invincible = true;
        invincibleTimer.Start(pDuration);
    }

    protected virtual void EndInvincible()
    {
        invincible = false;
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
