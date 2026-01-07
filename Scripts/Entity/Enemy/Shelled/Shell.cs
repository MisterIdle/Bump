using Com.IsartDigital.Shmup.NManager;
using Godot;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class Shell : Sprite2D
{
    [Export] public float protectAnimAngle = 20f;
    [Export] public float protectAnimTime = 0.1f;
    [Export] public float damageAnimAngle = 10f;
    [Export] public float damageAnimTime = 0.05f;
    [Export] public float destructionDistance = 0.05f;
    [Export] public float destructionPosTime = 0.2f;

    private float baseShellRotation = 0f;
    private ShaderMaterial material;
    private Color baseColor;

    private const string COLOR_CONST = "New";

    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        CheckMaterial();
        baseShellRotation = RotationDegrees;
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

    private void ChangeShellColor(Color pColor)
    {
        CheckMaterial();
        material?.SetShaderParameter(COLOR_CONST, pColor);
    }

    public void SetShellColor(Color pColor)
    {
        CheckMaterial();
        baseColor = pColor;
        material?.SetShaderParameter(COLOR_CONST, pColor);
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
        ChangeShellColor(Colors.DarkRed);

        lDamageTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation + damageAnimAngle, damageAnimTime);
        lDamageTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation - damageAnimAngle, damageAnimTime);
        lDamageTween.TweenProperty(this, (string)PropertyName.RotationDegrees, baseShellRotation, damageAnimTime);

        lDamageTween.Finished += () => ChangeShellColor(baseColor);
    }

    public void ShellDestructionAnimation()
    {
        Tween lDestructionTween = CreateTween();

        lDestructionTween.Parallel().TweenProperty(this, (string)PropertyName.Position + ":y", -destructionDistance, destructionPosTime)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);

        lDestructionTween.Finished += () => ChangeShellColor(Colors.Gray);
    }
}