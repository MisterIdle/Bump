using Godot;
using Com.IsartDigital.Shmup.NToolsBox;
using Com.IsartDigital.Shmup.NHUD;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NCollectibles;

public partial class Collectibles : Movable
{
    [Export] private float finalAlpha = 0f;
    [Export] private float rotationSpeed = 20f;
    [Export] private float animationSec = 2f;
    [Export] private float recoltHeight = 30f;
    [Export] private float recoltDuration = 0.2f;
    [Export] private float moveUpRatio = 0.6f;

    private Timer animationTimer;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void Initiate()
    {
        base.Initiate();

        Effect.BumpScale(this, baseScale);

        animationTimer = new Timer
        {
            WaitTime = animationSec,
            OneShot = false,
            Autostart = true
        };

        animationTimer.Timeout += PlayAnimation;
        AddChild(animationTimer);
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);
        velocity.Y = angleY;
        RotationDegrees += rotationSpeed * pDelta;
    }

    // ========================================================================
    // ANIMATION
    // ========================================================================

    private void PlayAnimation()
    {
        Effect.SquashStretch(this, baseScale, false);
        Effect.BumpScale(this, baseScale);
    }

    public void Recolt()
    {
        CollisionMask = 0;

        HUD.GetInstance().CreateBubbleScore(Position, score);

        Tween lTween = CreateTween();

        lTween.TweenProperty(this, (string)PropertyName.GlobalPosition + ":y", GlobalPosition.Y - recoltHeight,recoltDuration * moveUpRatio)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        lTween.TweenProperty(this, (string)PropertyName.Modulate + ":a", finalAlpha ,recoltDuration)
            .SetEase(Tween.EaseType.In);

        lTween.Finished += () => QueueFree();
    }

    // ========================================================================
    // COLLIDE
    // ========================================================================
    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);
        Recolt();
    }
}
