using Godot;
using System;

public partial class Entity : Movable
{
    public enum MoveDirection
    {
        Left,
        Right
    }

    [ExportCategory("Direction")]
    [Export] public MoveDirection direction;

    [ExportCategory("Alive")]
    [Export] public int health = 1;
    [Export] protected float invincibleTime = 1f;
    public bool invincible;

    [ExportCategory("Death")]
    [Export] public bool isDeath;
    [Export] protected float fallDeath = 20f;
    [Export] protected float deathJump = 5f;
    [Export] private float deathRotationSpeed = 60f;
    [Export] private float deathGravity = 20f;

    [ExportCategory("Visual")]
    [Export] protected PackedScene popScene;

    protected Timer invincibleTimer;

    public override void Initiate()
    {
        base.Initiate();

        invincibleTimer = new Timer();
        invincibleTimer.OneShot = true;
        invincibleTimer.Timeout += EndInvincible;

        AddChild(invincibleTimer);

        velocity = GetDirectionVector();
    }

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

        if (isDeath)
        {
            velocity += Vector2.Down * deathGravity * pDelta;
            RotationDegrees += deathRotationSpeed * Time.GetDelta(pDelta);
            return;
        }
        else
            AliveUpdate(Time.GetDelta(pDelta));
    }

    protected virtual void AliveUpdate(float pDelta) { }

    public virtual void EntityHit(int pDamage = 1)
    {
        if (invincible || isDeath) return;

        GD.Print(Name + " Hit");

        health -= pDamage;

        GD.Print(Name + "Hit: " + pDamage);

        if (health <= 0)
            EntityDeath();
    }

    public virtual void EntityDeath()
    {
        velocity = Vector2.Zero;
        isDeath = true;
        speed = fallDeath;

        CollisionMask = 0;
        CollisionLayer = 0;

        //Particule.Create(popScene, GlobalPosition);

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

    private Vector2 GetDirectionVector()
    {
        switch (direction)
        {
            case MoveDirection.Left:
                velocity = Vector2.Left;
                break;
            case MoveDirection.Right:
                velocity = Vector2.Right;
                break;
            default:
                break;
        }

        return velocity;
    }
}
