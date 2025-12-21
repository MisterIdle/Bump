using Godot;

public partial class Shell : GameObject
{
    [Export] public Sprite2D sprite;

    private float baseShellRotation;

    public override void Initiate()
    {
        base.Initiate();
        baseShellRotation = RotationDegrees;
    }

    public void SetShellColor(Color pColor)
    {
        sprite.SelfModulate = pColor;
    }

    public void ShellProtectAnimation()
    {
        Tween lTween = CreateTween();

        lTween.TweenProperty(this, Property.TWEEN_ROTATION_DEGREES, baseShellRotation + 20f, 0.1);
        lTween.TweenProperty(this, Property.TWEEN_ROTATION_DEGREES, baseShellRotation + -20f, 0.1);
        lTween.TweenProperty(this, Property.TWEEN_ROTATION_DEGREES, baseShellRotation, 0.1);
    }
}
