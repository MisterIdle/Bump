using Godot;

using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;

public partial class Spike : Projectiles
{
    protected Entity target;

    [Export] public GpuParticles2D particles;
    [Export] public CollisionShape2D collision;
    [Export] public bool isInstaKill = false;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void Initiate()
    {
        velocity = Vector2.Zero;
        base.Initiate();
    }

    public static Spike Create(PackedScene pScene, Entity pTarget, Vector2 pPos)
    {
        Spike lSpike = (Spike)pScene.Instantiate();

        lSpike.Position = pPos;
        lSpike.target = pTarget;

        lSpike.Initiate();

        return lSpike;

    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

        if (!isActive) return;

        Vector2 lDirection = Utils.DirectionTo(Position, target.Position);
        float lTargetAngle = lDirection.Angle();

        Rotation = Mathf.LerpAngle(Rotation, lTargetAngle, rotationSpeed * pDelta);
        velocity = Vector2.FromAngle(Rotation);
    }

    // ========================================================================
    // COLLIDE
    // ========================================================================

    protected override void OnCollide(Area2D pArea)
    {
        if (isActive && isInstaKill)
        {
            Player lPlayer = Player.GetInstance();
            lPlayer.InstantDeath();
        }

        base.OnCollide(pArea);

        if (!isActive)
            return;

        QueueFree();
    }

    protected override void Dispose(bool pDisposing)
    {
        Particule.Create(popScene, GlobalPosition);
    }
}
