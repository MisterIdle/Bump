using Godot;
using System;

using Com.IsartDigital.Shmup.NToolsBox;
using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity;

public partial class Entity : Movable
{
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

    // ========================================================================
    // INIT
    // ========================================================================

    public override void Initiate()
    {
        base.Initiate();

        invincibleTimer = new Timer();
        
        invincibleTimer.OneShot = true;
        invincibleTimer.Timeout += EndInvincible;

        AddChild(invincibleTimer);
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

        if (isDeath)
        {
            velocity += Vector2.Down * deathGravity * pDelta;
            RotationDegrees += deathRotationSpeed * ScaleTime.GetDelta(pDelta);
            return;
        }
        else
            AliveUpdate(ScaleTime.GetDelta(pDelta));
    }

    protected virtual void AliveUpdate(float pDelta) { }

    // ========================================================================
    // COLLIDE
    // ========================================================================

    protected virtual void StartInvincible(float pDuration)
    {
        invincible = true;
        invincibleTimer?.Start(pDuration);
    }

    protected virtual void EndInvincible()
    {
        invincible = false;
    }

    private void DeathBump()
    {
        velocity.Y = -deathJump;
    }

    public virtual void EntityHit(int pDamage = 1)
    {
        if (invincible || isDeath) return;

        AudioManager.GetInstance().Play(AudioManager.AudioType.EnemyExplosion);

        health -= pDamage;

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

        DeathBump();
    }
}
