using Godot;
using System;

public partial class Collectibles : Movable
{
    private float finalAlpha = 0;
    private float rotationSpeed = 20f;
    [Export] private float animationSec = 2f;
    private Timer animationTimer;

    public override void Initiate()
    {
        base.Initiate();
        velocity = Vector2.Left;
        Effect.BumpScale(this, baseScale);

        animationTimer = new Timer();

        animationTimer.WaitTime = animationSec;
        animationTimer.OneShot = false;
        animationTimer.Autostart = true;
        animationTimer.Timeout += PlayAnimation;

        AddChild(animationTimer);
    }

    public static Collectibles Create(PackedScene pScene, Vector2 pPos)
    {
        Collectibles collectible = (Collectibles)pScene.Instantiate();
        collectible.Position = pPos;

        GameManager.GetInstance().gameContainer.AddChild(collectible);
        collectible.Initiate();

        return collectible;
    }

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);
        velocity.Y = angleY;

        RotationDegrees += rotationSpeed * pDelta;
    }

    private void PlayAnimation()
    {
        Effect.SquashStretch(this, baseScale, false);
        Effect.BumpScale(this, baseScale);
    }

    public void Recolt(float pHeight = 30f, float pDuration = 0.2f)
    {
        CollisionMask = 0;

        Tween lTween = CreateTween();

        lTween.TweenProperty(this, (string)PropertyName.GlobalPosition + ":y", GlobalPosition.Y - pHeight, pDuration * 0.6f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        lTween.TweenProperty(this, (string)PropertyName.Modulate + ":a", finalAlpha, pDuration)
            .SetEase(Tween.EaseType.In);

        lTween.Finished += () => QueueFree();
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);
        Recolt();
    }
}
