using Com.IsartDigital.Shmup.NGameObject.NMovable.NCollectibles;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NHUD;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NStats;
using Com.IsartDigital.Shmup.NToolsBox;
using Godot;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class Enemy : Entity
{
    public enum ItemType { None, Health, Flower, Pow }

    [ExportCategory("Item")]
    [Export] private ItemType itemType = ItemType.None;

    [ExportCategory("Other")]
    [Export] protected int hammerScore = 1000;

    [Export] private int outOfBoundMargin = 250;

    public float cameraShakeIntensity = 4f;
    public float cameraShakeDuration = 0.5f;
    public float cameraShakeFrequency = 10f;
    public float cameraShakeRandomness = 2f;

    [Export] public float squashStretchDuration = 0.4f;

    [Export] private float flipCooldownDuration = 0.3f;
    private Timer flipTimer;

    private int bossDamage = 20;
    private int clearAll = 9999;
    protected const int DEFAULT_DAMAGE = 1;

    protected bool killedByHammer;
    protected float globalPlayerDistance;

    private static ulong lastKillTimeMsec;
    private static int comboMultiplier = 1;
    private const double COMBO_WINDOW_MSEC = 800.0;

    public override void _Ready()
    {
        base._Ready();

        flipTimer = new Timer();
        flipTimer.OneShot = true;
        AddChild(flipTimer);
    }

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

        globalPlayerDistance = Utils.DistanceTo(this, Player.GetInstance());

        if (Utils.IsOutOfBound(Position, outOfBoundMargin))
        {
            health = 0;
            QueueFree();
        }
    }

    public static void ClearAll(bool pBossOneShot, bool pTotalClear)
    {
        GameManager lGameManager = GameManager.GetInstance();

        if (lGameManager == null || !IsInstanceValid(lGameManager)) return;

        if (lGameManager.enemyContainer == null) return;

        foreach (Enemy lEnemy in lGameManager.enemyContainer.GetChildren())
        {
            if (IsInstanceValid(lEnemy) && lEnemy.enable)
            {
                if (pTotalClear)
                {
                    lEnemy.QueueFree();
                    continue;
                }

                if (lEnemy is KingKoops && !pBossOneShot)
                {
                    lEnemy.EntityHit(lEnemy.bossDamage);
                    continue;
                }

                lEnemy.health = 0;
                lEnemy.EntityHit(lEnemy.clearAll);
            }
        }
    }

    protected void Flip(bool pFaceLeft)
    {
        if (!flipTimer.IsStopped()) return;

        float lCurrentDirection = Mathf.Sign(Scale.X);

        float lDirection = pFaceLeft ? 1 : -1;

        if (lCurrentDirection == lDirection) return;

        Scale = new Vector2(Mathf.Abs(baseScale.X) * lDirection, baseScale.Y);
        baseScale = Scale;

        Effect.SquashStretch(this, baseScale, true);

        flipTimer.Start(flipCooldownDuration);
    }

    public override void EntityHit(int pDamage)
    {
        base.EntityHit(pDamage);
        StartInvincible(invincibleTime);
    }

    protected override void StartInvincible(float pDuration)
    {
        base.StartInvincible(pDuration);
        SetCollisionMaskValue(1, false);
        SetCollisionMaskValue(9, false);
    }

    protected override void EndInvincible()
    {
        base.EndInvincible();
        SetCollisionMaskValue(1, true);
        SetCollisionMaskValue(9, true);
    }

    protected int GetScoreWithCombo(int pBaseScore)
    {
        ulong lCurrentTime = Time.GetTicksMsec();

        if (lCurrentTime - lastKillTimeMsec <= COMBO_WINDOW_MSEC)
            comboMultiplier++;
        else
            comboMultiplier = 1;

        lastKillTimeMsec = lCurrentTime;

        return pBaseScore * comboMultiplier;
    }

    public override void EntityDeath()
    {
        invincibleTimer.Stop();

        EndInvincible();

        base.EntityDeath();

        GameStats.enemyKills++;

        GameManager.GetInstance().SpawnCollectible(itemType, GlobalPosition);

        SetCollisionMaskValue(1, false);
        SetCollisionMaskValue(9, false);
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        AudioManager.GetInstance().Play(AudioManager.AudioType.EnemyExplosionNonBug);

        if (pArea.Owner is Hammer)
        {
            killedByHammer = true;
            EntityHit(Hammer.GetInstance().attackForce);

            Effect.Shake(cameraShakeIntensity, cameraShakeDuration, cameraShakeFrequency, cameraShakeRandomness);
            ScaleTime.SlowMotion();
            Effect.SquashStretch(this, baseScale, false, false, squashStretchDuration);

            Particule.Create(popScene, Position);
        }
        else
        {
            killedByHammer = false;
            EntityHit(DEFAULT_DAMAGE);
        }
    }
}