using Godot;

using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NToolsBox;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;

public partial class Pow : GameObject
{
    [Export] private float dropDuration = 1.0f;
    [Export] private float scaleMultiplier = 20f;
    [Export] private float killDelay = 0.01f;

    [Export] private float entranceOvershoot = -50f;
    [Export] private float entranceXOffsetTarget = 250f;
    [Export] private float minSpawnOffset = 100f;
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

        Vector2 screenCenter = GameManager.GetInstance().screenSizeCenterX;

        float spawnY = (GameManager.rand.Randf() < 0.5f) ? -lPow.minSpawnOffset : GameManager.GetInstance().screenSize.Y + lPow.minSpawnOffset;
        float spawnX = screenCenter.X;

        lPow.Position = new Vector2(spawnX, spawnY);

        float targetY = GameManager.GetInstance().screenSize.Y / 2;
        lPow.targetPosition = new Vector2(screenCenter.X + lPow.entranceXOffsetTarget, targetY);

        lPow.Initiate();

        return lPow;
    }

    // ========================================================================
    // ANIMATION
    // ========================================================================

    private void StartEntranceAnimation()
    {
        Tween lTween = CreateTween();

        Vector2 overshootPos = targetPosition + new Vector2(0, entranceOvershoot);
        lTween.TweenProperty(this, (string)PropertyName.Position, overshootPos, dropDuration * 0.7f)
              .SetTrans(Tween.TransitionType.Sine)
              .SetEase(Tween.EaseType.Out);

        lTween.TweenProperty(this, (string)PropertyName.Position, targetPosition, dropDuration * 0.3f)
              .SetTrans(Tween.TransitionType.Bounce)
              .SetEase(Tween.EaseType.In);

        lTween.Finished += PostExplosion;
    }

    private void PostExplosion()
    {
        Tween lTween = Effect.SquashStretch(sprite, baseSpriteScale, false, true, 0.5f);

        Effect.FlashColor(this, Colors.Red);
        Effect.Shake(35, 1.2f);
        ScaleTime.SlowMotion();

        Particule.Create(pop, Position);

        lTween.Finished += StartExplosionSequence;
    }

    private void StartExplosionSequence()
    {
        Projectiles.ClearAll();
        Enemy.ClearAll(false, false);
        QueueFree();
    }

}
