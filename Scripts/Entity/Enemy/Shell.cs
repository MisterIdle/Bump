using Godot;

public partial class Shell : Sprite2D
{
    public ShelledEnemy ownerEnemy;

    private float baseShellRotation = 0f;
    [Export] public float protectAnimAngle = 20f;
    [Export] public float protectAnimTime = 0.1f;
    [Export] public float damageAnimAngle = 10f;
    [Export] public float damageAnimTime = 0.05f;

    private ShaderMaterial material;
    private const string COLOR_CONST = "New";

    private Color baseColor;


    public override void _Ready()
    {
        material = (ShaderMaterial)Material.Duplicate();
        Material = material;

        baseShellRotation = RotationDegrees;
    }

    public void SetShellColor(Color pColor)
    {
        baseColor = pColor;
        material.SetShaderParameter(COLOR_CONST, pColor);
    }

    public void ChangeShellColor(Color pColor)
    {
        material.SetShaderParameter(COLOR_CONST, pColor);
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

        ChangeShellColor(Colors.Red);

        lTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation + damageAnimAngle, damageAnimTime);
        lTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation - damageAnimAngle, damageAnimTime);
        lTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation, damageAnimTime);

        lTween.Finished += () => ChangeShellColor(baseColor);
    }

    public void ShellDestructionAnimation()
    {
        Tween lTween = CreateTween();

        lTween.Parallel().TweenProperty(this, (string)PropertyName.Position + ":y", Position.Y * 2f, 0.2f)
              .SetTrans(Tween.TransitionType.Sine)
              .SetEase(Tween.EaseType.Out);

        lTween.Finished += ShellDisable;
    }
    
    private void ShellDisable()
    {
        ChangeShellColor(Colors.Gray);
    }
}
