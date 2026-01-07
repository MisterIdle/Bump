using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NToolsBox;
using Godot;

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

    private int index;

    private Vector2 targetPosition;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        StartEntranceAnimation();
    }

    public static Pow Create(PackedScene pScene)
    {
        Pow lPow = (Pow)pScene.Instantiate();

        Vector2 lScreenCenter = GameManager.GetInstance().screenSizeCenterX;

        float lSpawnY = (GameManager.rand.Randf() < 0.5f) ? -lPow.minSpawnOffset : GameManager.GetInstance().screenSize.Y + lPow.minSpawnOffset;
        float lSpawnX = lScreenCenter.X;

        lPow.Position = new Vector2(lSpawnX, lSpawnY);

        float lTargetY = GameManager.GetInstance().screenSize.Y / 2;
        lPow.targetPosition = new Vector2(lScreenCenter.X + lPow.entranceXOffsetTarget, lTargetY);

        lPow.Initiate();

        return lPow;
    }

    // ========================================================================
    // ANIMATION
    // ========================================================================

    private void StartEntranceAnimation()
    {
        Tween lShowTween = CreateTween();

        Vector2 lEntrancePos = targetPosition + new Vector2(0, entranceMore);
        lShowTween.TweenProperty(this, (string)PropertyName.Position, lEntrancePos, dropDuration * 0.7f)
              .SetTrans(Tween.TransitionType.Sine)
              .SetEase(Tween.EaseType.Out);

        lShowTween.TweenProperty(this, (string)PropertyName.Position, targetPosition, dropDuration * 0.3f)
              .SetTrans(Tween.TransitionType.Bounce)
              .SetEase(Tween.EaseType.In);

        lShowTween.Finished += PostExplosion;
    }

    private void PostExplosion()
    {
        Tween lExplosionTween = Effect.SquashStretch(sprite, baseSpriteScale, false, true, 0.5f);

        Effect.FlashColor(this, Colors.Red);
        Effect.Shake(shakeForce, shakeDuration);
        ScaleTime.SlowMotion();

        Particule.Create(pop, Position);

        lExplosionTween.Finished += StartExplosionSequence;
    }

    private void StartExplosionSequence()
    {
        Projectiles.ClearAll();
        Enemy.ClearAll(false, false);

        QueueFree();
    }
}
