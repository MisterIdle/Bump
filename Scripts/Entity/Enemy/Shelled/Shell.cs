using Godot;

public partial class Shell : Sprite2D
{
    [Export] public float protectAnimAngle = 20f;
    [Export] public float protectAnimTime = 0.1f;
    [Export] public float damageAnimAngle = 10f;
    [Export] public float damageAnimTime = 0.05f;

    private float baseShellRotation = 0f;
    private ShaderMaterial material;
    private Color baseColor;
    private const string COLOR_CONST = "New";

    public override void _Ready()
    {
        CheckMaterial();
        baseShellRotation = RotationDegrees;
    }

    private void CheckMaterial()
    {
        if (material != null) return;
        if (Material is ShaderMaterial sm)
        {
            material = (ShaderMaterial)sm.Duplicate();
            Material = material;
        }
    }

    public void SetShellColor(Color pColor)
    {
        CheckMaterial();
        baseColor = pColor;
        material?.SetShaderParameter(COLOR_CONST, pColor);
    }

    public void ChangeShellColor(Color pColor)
    {
        CheckMaterial();
        material?.SetShaderParameter(COLOR_CONST, pColor);
    }

    public void ShellProtectAnimation()
    {
        AudioManager.GetInstance().Play(AudioManager.AudioType.EnemyExplosion, 2.5f);

        Tween lTween = CreateTween();
        lTween.TweenProperty(this, "rotation_degrees", baseShellRotation + protectAnimAngle, protectAnimTime);
        lTween.TweenProperty(this, "rotation_degrees", baseShellRotation - protectAnimAngle, protectAnimTime);
        lTween.TweenProperty(this, "rotation_degrees", baseShellRotation, protectAnimTime);
    }

    public void ShellDamageAnimation()
    {
        Tween lTween = CreateTween();
        ChangeShellColor(Colors.Red);

        lTween.TweenProperty(this, "rotation_degrees", baseShellRotation + damageAnimAngle, damageAnimTime);
        lTween.TweenProperty(this, "rotation_degrees", baseShellRotation - damageAnimAngle, damageAnimTime);
        lTween.TweenProperty(this, "rotation_degrees", baseShellRotation, damageAnimTime);

        lTween.Finished += () => ChangeShellColor(baseColor);
    }

    public void ShellDestructionAnimation()
    {
        Tween lTween = CreateTween();
        lTween.Parallel().TweenProperty(this, "position:y", Position.Y * 0.5f, 0.2f)
              .SetTrans(Tween.TransitionType.Sine)
              .SetEase(Tween.EaseType.Out);

        lTween.Finished += () => ChangeShellColor(Colors.Gray);
    }
}
