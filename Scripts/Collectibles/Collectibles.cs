using Godot;
using System;

public partial class Collectibles : Movable
{
    private float finalAlpha = 0;

    private Sprite2D parachute;
    [Export] private NodePath parachutePath;

    private Tween parachuteTween;

    public override void Initiate()
    {
        base.Initiate();

        if (parachutePath != null) 
            parachute = (Sprite2D)GetNode(parachutePath);

        velocity = Vector2.Down;
    }


    public void StartParachuteEffect(float pRotation = 8f, float pDuration = 1f, float pHeightOffset = 0.1f)
    {
        parachuteTween?.Kill();
        parachuteTween = CreateTween();
        parachuteTween.SetLoops(0);

        parachuteTween
            .TweenProperty(this, (string)PropertyName.RotationDegrees, pRotation, pDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        parachuteTween
            .TweenProperty(this, (string)PropertyName.RotationDegrees, -pRotation, pDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    public void Recolt(float pHeight = 30f, float pDuration = 0.2f)
    {
        CollisionMask = 0;

        if (parachute != null)
            parachute.Visible = false;

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
