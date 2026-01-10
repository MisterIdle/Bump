using Godot;
using System.Collections.Generic;

using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NHUD;

public partial class MainMenu : MenuBase
{
    [ExportCategory("References")]
    [Export] private Label title;

    [ExportSubgroup("Buttons")]
    [Export] private Button startButton;
    [Export] private Button optionButton;
    [Export] private Button helpButton;
    [Export] private Button creditsButton;
    [Export] private Button quitButton;

    [ExportCategory("Settings")]
    [Export] private float introLetterSpeed = 0.08f;
    [Export] private float letterSpacing = 1f;

    [ExportGroup("Wave")]
    [Export] private float waveAmplitude = 4f;
    [Export] private float waveDuration = 2.0f;
    [Export] private float waveFrequencyDelay = 0.1f;
    [Export] private float rotationAmplitude = 3f;

    private List<Label> generatedLetterList = new List<Label>();
    private List<Tween> waveTweensList = new List<Tween>();
    private Tween introTitleTween;

    private const string THEME_FONT_NAME = "font";
    private const int DEFAULT_WIDTH_LIMIT = -1;
    private const float HALF_MULTIPLIER = 0.5f;
    private const float ZERO_ROTATION = 0f;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        PrepareTitleText();

        optionButton.Pressed += OnOptionPressed;
        startButton.Pressed += OnStartPressed;
        helpButton.Pressed += OnHelpPressed;
        quitButton.Pressed += OnQuitPressed;
        creditsButton.Pressed += OnCreditsPressed;
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    public override void OnEnter()
    {
        base.OnEnter();

        ResetTitleVisuals();
        StartTitleWaveLoop();

        startButton.GrabFocus();

        AudioManager.GetInstance().PlayMusic(AudioManager.MusicType.Menu);
    }

    public override void OnExit()
    {
        base.OnExit();
        introTitleTween?.Kill();

        foreach (Tween lTween in waveTweensList)
            if (lTween != null && lTween.IsValid()) 
                lTween.Kill();

        waveTweensList.Clear();

        PlayGenericExitAnimation();
    }

    // ========================================================================
    // ACTION
    // ========================================================================

    private void OnOptionPressed()
    {
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.Options);
        AudioManager.GetInstance().Play(AudioManager.AudioType.UIClic);
    }

    private void OnStartPressed()
    {
        AudioManager.GetInstance().Play(AudioManager.AudioType.UIStart);
        MenuManager.GetInstance().nextIsGame = true;
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.Help);
        HUD.GetInstance().ResetHUD();
    }

    private void OnHelpPressed()
    {
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.Help);
        AudioManager.GetInstance().Play(AudioManager.AudioType.UIClic);
    }

    private void OnCreditsPressed()
    {
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.Credits);
        AudioManager.GetInstance().Play(AudioManager.AudioType.UIClic);
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
        AudioManager.GetInstance().Play(AudioManager.AudioType.UIClic);
    }

    // ========================================================================
    // ANIMATION TITLE
    // ========================================================================

    private void ResetTitleVisuals()
    {
        foreach (Label lLabel in generatedLetterList)
        {
            lLabel.Scale = Vector2.Zero;
            lLabel.Modulate = Colors.Transparent;
            lLabel.RotationDegrees = ZERO_ROTATION;
            lLabel.Position = new Vector2(lLabel.Position.X, 0);
        }
    }

    private void PrepareTitleText()
    {
        if (title == null) return;

        string lContent = title.Text;
        title.Visible = false;

        HBoxContainer lContainer = new HBoxContainer();
        lContainer.Alignment = BoxContainer.AlignmentMode.Center;
        lContainer.Position = title.Position;
        lContainer.Size = title.Size;
        lContainer.MouseFilter = MouseFilterEnum.Ignore;

        title.GetParent().CallDeferred(Node.MethodName.AddChild, lContainer);

        foreach (char lLetter in lContent)
        {
            Control lWrapper = new Control();
            Label lLetterLabel = new Label();

            lLetterLabel.Text = lLetter.ToString();
            lLetterLabel.LabelSettings = title.LabelSettings;
            lLetterLabel.Modulate = Colors.Transparent;

            Font lFontResource = lLetterLabel.GetThemeFont(THEME_FONT_NAME);

            if (lFontResource != null)
            {
                Vector2 lCharSize = lFontResource.GetStringSize(lLetterLabel.Text, lLetterLabel.HorizontalAlignment, DEFAULT_WIDTH_LIMIT, lLetterLabel.LabelSettings.FontSize);
                lLetterLabel.PivotOffset = new Vector2(lCharSize.X / 2, lLetterLabel.LabelSettings.FontSize / 2);
                Vector2 lWrapperSize = lCharSize * letterSpacing;
                lWrapper.CustomMinimumSize = lWrapperSize;
            }

            lWrapper.AddChild(lLetterLabel);
            lContainer.AddChild(lWrapper);
            generatedLetterList.Add(lLetterLabel);
        }
    }

    private void StartTitleWaveLoop()
    {
        foreach (Tween lTween in waveTweensList)
        {
            if (lTween != null && lTween.IsValid()) lTween.Kill();
        }
        waveTweensList.Clear();

        for (int i = 0; i < generatedLetterList.Count; i++)
        {
            Label lLabel = generatedLetterList[i];
            float lDelayOffset = i * waveFrequencyDelay;

            Tween lWaveTween = CreateTween();
            waveTweensList.Add(lWaveTween);

            lWaveTween.SetLoops();

            float lHalfDuration = waveDuration * HALF_MULTIPLIER;

            lWaveTween.TweenProperty(lLabel, (string)PropertyName.Position + ":y", -waveAmplitude, lHalfDuration)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.InOut)
                .SetDelay(lDelayOffset);

            lWaveTween.Parallel().TweenProperty(lLabel, (string)PropertyName.RotationDegrees, rotationAmplitude, lHalfDuration)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.InOut)
                .SetDelay(lDelayOffset);

            lWaveTween.Chain().TweenProperty(lLabel, (string)PropertyName.Position + ":y", waveAmplitude, lHalfDuration)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.InOut);

            lWaveTween.Parallel().TweenProperty(lLabel, (string)PropertyName.RotationDegrees, -rotationAmplitude, lHalfDuration)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.InOut);
        }
    }
}