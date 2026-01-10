using Godot;

using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NStats;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;

public partial class Pow : GameObject
{
    [Export] private float dropDuration = 1.0f;
    [Export] private float scaleMultiplier = 20f;

    [Export] private float entranceMore = -50f;
    [Export] private float entranceXOffsetTarget = 250f;
    [Export] private float minSpawnOffset = 100f;

    [Export] private float shakeForce = 35;
    [Export] private float shakeDuration = 1.2f;

    [Export] private PackedScene pop;

    private Vector2 targetPosition;

    private const float TWEEN_APPROACH_RATIO = 0.7f;
    private const float TWEEN_LAND_RATIO = 0.3f;
    private const float SPAWN_RANDOM_THRESHOLD = 0.5f;
    private const float SQUASH_RATIO = 0.5f;

    public override void _Ready()
    {
        base._Ready();
        StartEntranceAnimation();

        GameStats.powerUsed++;
    }

    public static Pow Create(PackedScene pScene)
    {
        Pow lPow = (Pow)pScene.Instantiate();
        Camera lCamera = Camera.GetInstance();

        Vector2 lCenter = lCamera.GetScreenCenter();
        float lSpawnY;

        if (GameManager.rand.Randf() < SPAWN_RANDOM_THRESHOLD)
            lSpawnY = lCamera.GetTop() - lPow.minSpawnOffset;
        else
            lSpawnY = lCamera.GetBottom() + lPow.minSpawnOffset;

        lPow.Position = new Vector2(lCenter.X, lSpawnY);
        lPow.targetPosition = new Vector2(lCenter.X + lPow.entranceXOffsetTarget, lCenter.Y);

        lPow.Initiate();

        return lPow;
    }

    private void StartEntranceAnimation()
    {
        Tween lShowTween = CreateTween();

        Vector2 lEntrancePos = targetPosition + new Vector2(0, entranceMore);
        string lPropertyPos = (string)PropertyName.Position;

        lShowTween.TweenProperty(this, lPropertyPos, lEntrancePos, dropDuration * TWEEN_APPROACH_RATIO)
              .SetTrans(Tween.TransitionType.Sine)
              .SetEase(Tween.EaseType.Out);

        lShowTween.TweenProperty(this, lPropertyPos, targetPosition, dropDuration * TWEEN_LAND_RATIO)
              .SetTrans(Tween.TransitionType.Bounce)
              .SetEase(Tween.EaseType.In);

        lShowTween.Finished += PostExplosion;
    }

    private void PostExplosion()
    {
        Tween lExplosionTween = Effect.SquashStretch(sprite, baseSpriteScale, false, true, SQUASH_RATIO);

        Effect.FlashColor(this, Colors.Red);
        Effect.Shake(shakeForce, shakeDuration);

        ScaleTime.SlowMotion();

        if (pop != null) Particule.Create(pop, Position);

        lExplosionTween.Finished += StartExplosionSequence;
    }

    private void StartExplosionSequence()
    {
        Projectiles.ClearAll();
        Enemy.ClearAll(false, false);

        QueueFree();
    }
}
