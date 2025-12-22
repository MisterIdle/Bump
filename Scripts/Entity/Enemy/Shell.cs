using Godot;

public partial class Shell : Sprite2D
{
    [Export] public Sprite2D sprite;
    public ShelledEnemy ownerEnemy;

    [Export] public Color defaultColor = Colors.White;
    [Export] public Color damageColor = Colors.DarkRed;
    [Export] public Color endDamageColor = Colors.White;
    [Export] public Color deathColor = Colors.Gray;

    private float baseShellRotation = 0f;
    [Export] public float protectAnimAngle = 20f;
    [Export] public float protectAnimTime = 0.1f;
    [Export] public float damageAnimAngle = 10f;
    [Export] public float damageAnimTime = 0.05f;

    public override void _Ready()
    {
        baseShellRotation = RotationDegrees;
    }

    public void SetShellColor(Color color)
    {
        sprite.SelfModulate = color;
    }

    public void ShellProtectAnimation()
    {
        Tween lTween = CreateTween();
        lTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation + protectAnimAngle, protectAnimTime);
        lTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation - protectAnimAngle, protectAnimTime);
        lTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation, protectAnimTime);
    }

    public void ShellDamageAnimation()
    {
        Tween lTween = CreateTween();

        sprite.SelfModulate = damageColor;

        lTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation + damageAnimAngle, damageAnimTime);
        lTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation - damageAnimAngle, damageAnimTime);
        lTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation, damageAnimTime);

        lTween.TweenProperty(sprite, (string)PropertyName.SelfModulate, endDamageColor, protectAnimTime);
    }

    public void ShellDestructionAnimation()
    {
        Tween lTween = CreateTween();

        sprite.SelfModulate = deathColor;

        lTween.Parallel().TweenProperty(this, (string)PropertyName.Position + ":y", Position.Y * 2f, 0.2f)
              .SetTrans(Tween.TransitionType.Sine)
              .SetEase(Tween.EaseType.Out);

        lTween.Finished += ShellDisable;
    }
    
    private void ShellDisable()
    {
        Tween lTween = CreateTween();

        lTween.Parallel().TweenProperty(sprite, (string)PropertyName.SelfModulate, deathColor, 0.1f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }
}
