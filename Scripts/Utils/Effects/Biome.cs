using Godot;
using System;

namespace Com.IsartDigital.Shmup.NToolsBox;

public partial class Biome : Control
{
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

    private const string STYLEBOX_PANEL = "panel";

    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        SetupOutputTexture();

        SetBiome(BiomeType.MainMenu);
    }

    // ========================================================================
    // ACTION
    // ========================================================================

    public void SetBiome(BiomeType pBiomeType)
    {
        if (baseTexture == null) return;

        GradientTexture2D lTargetTexture = GetTextureForBiome(pBiomeType);
        Gradient lTargetGradient = lTargetTexture.Gradient;
        Gradient lStartGradient = (Gradient)baseTexture.Gradient.Duplicate();

        if (tween != null && tween.IsValid()) tween.Kill();
        tween = CreateTween();

        tween.TweenMethod(Callable.From<float>((pTime) => { LerpGradients(lStartGradient, lTargetGradient, pTime); }), 0.0f, 1.0f, TransitionDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    private GradientTexture2D GetTextureForBiome(BiomeType pType)
    {
        switch (pType)
        {
            case BiomeType.MainMenu: 
                return GradientMainMenu;
            case BiomeType.Overworld: 
                return GradientOverworld;
            case BiomeType.Snow: 
                return GradientSnow;
            case BiomeType.Boss: 
                return GradientBoss;
            default: 
                return GradientMainMenu;
        }
    }

    private void LerpGradients(Gradient pFrom, Gradient pTo, float pWeight)
    {
        Color[] lNewColors = new Color[pFrom.Colors.Length];
        float[] lNewOffsets = new float[pFrom.Offsets.Length];

        for (int i = 0; i < pFrom.Colors.Length; i++)
        {
            int lTargetIndex = Math.Min(i, pTo.Colors.Length - 1);

            lNewColors[i] = pFrom.Colors[i].Lerp(pTo.Colors[lTargetIndex], pWeight);
            lNewOffsets[i] = Mathf.Lerp(pFrom.Offsets[i], pTo.Offsets[lTargetIndex], pWeight);
        }

        baseTexture.Gradient.Colors = lNewColors;
        baseTexture.Gradient.Offsets = lNewOffsets;
    }

    private void SetupOutputTexture()
    {
        if (panel == null) return;

        StyleBoxTexture lStylebox = (StyleBoxTexture)panel.GetThemeStylebox(STYLEBOX_PANEL);

        if (lStylebox != null && lStylebox.Texture is GradientTexture2D)
        {
            baseTexture = (GradientTexture2D)lStylebox.Texture.Duplicate();
            lStylebox.Texture = baseTexture;
        }
    }
}