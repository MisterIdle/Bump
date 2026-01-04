using Godot;
using System;

public partial class Biome : Control
{
    private static Biome instance;
    public static Biome GetInstance()
    {
        return instance;
    }

    public enum BiomeType
    {
        MainMenu,
        Overworld,
        Snow,
        Boss
    }

    [ExportGroup("Biomes Presets")]
    [Export] public GradientTexture2D GradientMainMenu;
    [Export] public GradientTexture2D GradientOverworld;
    [Export] public GradientTexture2D GradientSnow;
    [Export] public GradientTexture2D GradientBoss;

    [ExportGroup("Settings")]
    [Export] public Panel panel;
    [Export] public float TransitionDuration = 2.0f;

    private GradientTexture2D baseTexture;
    private Tween tween;


    public override void _Ready()
    {
        if (instance == null) instance = this;
        else
        {
            QueueFree();
            return;
        }

        SetupOutputTexture();

        SetBiome(BiomeType.MainMenu);
    }

    public void SetBiome(BiomeType pBiomeType)
    {
        if (baseTexture == null) return;

        GradientTexture2D targetTexture = GetTextureForBiome(pBiomeType);

        Gradient targetGradient = targetTexture.Gradient;

        Gradient startGradient = (Gradient)baseTexture.Gradient.Duplicate();

        tween?.Kill();
        tween = CreateTween();

        tween.TweenMethod(Callable.From<float>((pTime) => { LerpGradients(startGradient, targetGradient, pTime); }), 0.0f, 1.0f, TransitionDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    private GradientTexture2D GetTextureForBiome(BiomeType pType)
    {
        switch (pType)
        {
            case BiomeType.MainMenu: return GradientMainMenu;
            case BiomeType.Overworld: return GradientOverworld;
            case BiomeType.Snow: return GradientSnow;
            case BiomeType.Boss: return GradientBoss;
            default: return GradientMainMenu;
        }
    }

    private void LerpGradients(Gradient pFrom, Gradient pTo, float pWeight)
    {
        Color[] newColors = new Color[pFrom.Colors.Length];
        float[] newOffsets = new float[pFrom.Offsets.Length];

        for (int i = 0; i < pFrom.Colors.Length; i++)
        {
            int targetIndex = Math.Min(i, pTo.Colors.Length - 1);

            newColors[i] = pFrom.Colors[i].Lerp(pTo.Colors[targetIndex], pWeight);
            newOffsets[i] = Mathf.Lerp(pFrom.Offsets[i], pTo.Offsets[targetIndex], pWeight);
        }

        baseTexture.Gradient.Colors = newColors;
        baseTexture.Gradient.Offsets = newOffsets;
    }

    private void SetupOutputTexture()
    {
        StyleBoxTexture lStylebox = (StyleBoxTexture)panel.GetThemeStylebox("panel");
        if (lStylebox.Texture is GradientTexture2D)
        {
            baseTexture = (GradientTexture2D)lStylebox.Texture.Duplicate();
            lStylebox.Texture = baseTexture;
        }
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}