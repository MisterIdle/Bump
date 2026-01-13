using Godot;

using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class Shell : Sprite2D
{
    [ExportGroup("Animation Settings")]
    [Export] public float protectAnimAngle = 20f;
    [Export] public float protectAnimTime = 0.1f;

    [Export] public float damageAnimAngle = 10f;
    [Export] public float damageAnimTime = 0.05f;

    [Export] public float destructionDistance = 10f;
    [Export] public float destructionPosTime = 0.5f;

    [ExportGroup("Color Settings")]
    [Export] public float colorTransitionDuration = 0.2f;
    [Export] public float damageFlashDuration = 0.1f;

    private float baseShellRotation = 0f;
    private ShaderMaterial material;
    private Color baseColor;

    private Tween colorTween;

    private const string COLOR_CONST = "New";
    private const string SHADER_PARAM_PATH = "shader_parameter/" + COLOR_CONST;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        CheckMaterial();
        baseShellRotation = RotationDegrees;

        if (material != null)
        {
            baseColor = (Color)material.GetShaderParameter(COLOR_CONST);
        }
    }

    private void CheckMaterial()
    {
        if (material != null) return;

        if (Material is ShaderMaterial lShaderMaterial)
        {
            material = (ShaderMaterial)lShaderMaterial.Duplicate();
            Material = material;
        }
    }

    // ========================================================================
    // ANIMATION & COLORS
    // ========================================================================

    private void ShellColor(Color pTargetColor, float pDuration)
    {
        CheckMaterial();
        if (material == null) return;

        colorTween?.Kill();
        colorTween = CreateTween();

        colorTween.TweenProperty(material, SHADER_PARAM_PATH, pTargetColor, pDuration)
                  .SetTrans(Tween.TransitionType.Sine)
                  .SetEase(Tween.EaseType.Out);
    }

    public void SetShellColor(Color pColor)
    {
        baseColor = pColor;
        ShellColor(baseColor, colorTransitionDuration);
    }

    public Color GetShellColor()
    {
        return baseColor;
    }

    public void ShellProtectAnimation()
    {
        float pPitch = 3.5f;
        AudioManager.GetInstance().Play(AudioManager.AudioType.EnemyExplosion, pPitch);

        Tween lProtectTween = CreateTween();

        lProtectTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation + protectAnimAngle, protectAnimTime);
        lProtectTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation - protectAnimAngle, protectAnimTime);
        lProtectTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation, protectAnimTime);
    }

    public void ShellDamageAnimation()
    {
        Tween lDamageTween = CreateTween();

        ShellColor(Colors.DarkRed, damageFlashDuration);

        lDamageTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation + damageAnimAngle, damageAnimTime);
        lDamageTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation - damageAnimAngle, damageAnimTime);
        lDamageTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation, damageAnimTime);

        lDamageTween.Finished += () => ShellColor(baseColor, colorTransitionDuration);
    }

    public void ShellDestructionAnimation()
    {
        Tween lDestructionTween = CreateTween();

        lDestructionTween.Parallel().TweenProperty(this, (string)PropertyName.Position + ":y", -destructionDistance, destructionPosTime)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);

        lDestructionTween.Finished += () => ShellColor(Colors.Gray, colorTransitionDuration);
    }
}