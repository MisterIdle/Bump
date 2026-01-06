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

    private List<Label> generatedLetterLabels = new List<Label>();
    private Tween introTitleTween;
    private List<Tween> waveTweens = new List<Tween>();

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
        PlayTitleIntroAnimation();

        startButton.GrabFocus();

        AudioManager.GetInstance().PlayMusic(AudioManager.MusicType.Menu);
    }

    public override void OnExit()
    {
        base.OnExit();

        introTitleTween?.Kill();

        foreach (Tween lTween in waveTweens)
        {
            lTween?.Kill();
        }
        waveTweens.Clear();

        PlayGenericExitAnimation();
    }

    // ========================================================================
    // ACTION
    // ========================================================================

    private void OnOptionPressed()
    {
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.Options);
    }

    private void OnStartPressed()
    {
        MenuManager.GetInstance().nextIsGame = true;
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.Help);
    }

    private void OnHelpPressed()
    {
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.Help);
    }

    private void OnCreditsPressed()
    {
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.Credits);
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }

    // ========================================================================
    // ANIMATION TITLE
    // ========================================================================

    private void ResetTitleVisuals()
    {
        foreach (var lLabel in generatedLetterLabels)
        {
            lLabel.Scale = Vector2.Zero;
            lLabel.Modulate = new Color(1, 1, 1, 0);
            lLabel.RotationDegrees = 0;
            lLabel.Position = new Vector2(lLabel.Position.X, 0);
        }
    }

    private void PrepareTitleText()
    {
        if (title == null) return;

        string lContent = title.Text;
        title.Visible = false;
        string lFont = "font";

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
            lLetterLabel.Modulate = new Color(1, 1, 1, 0);

            var font = lLetterLabel.GetThemeFont(lFont);
            if (font != null)
            {
                Vector2 lCharSize = font.GetStringSize(lLetterLabel.Text, lLetterLabel.HorizontalAlignment, -1, lLetterLabel.LabelSettings.FontSize);
                lLetterLabel.PivotOffset = new Vector2(lCharSize.X / 2, lLetterLabel.LabelSettings.FontSize / 2);
                Vector2 lWrapperSize = lCharSize * letterSpacing;
                lWrapper.CustomMinimumSize = lWrapperSize;
            }

            lWrapper.AddChild(lLetterLabel);
            lContainer.AddChild(lWrapper);
            generatedLetterLabels.Add(lLetterLabel);
        }
    }

    private void PlayTitleIntroAnimation()
    {
        introTitleTween = CreateTween();
        introTitleTween.SetParallel(true);

        for (int i = 0; i < generatedLetterLabels.Count; i++)
        {
            Label lLabel = generatedLetterLabels[i];
            float lDelay = i * introLetterSpeed;

            introTitleTween.TweenProperty(lLabel, (string)PropertyName.Scale, Vector2.One, 0.4f)
                .SetDelay(lDelay)
                .SetTrans(Tween.TransitionType.Back);

            introTitleTween.TweenProperty(lLabel, (string)PropertyName.Modulate + ":a", 1f, 0.3f)
                .SetDelay(lDelay);
        }

        introTitleTween.Chain().TweenCallback(Callable.From(StartTitleWaveLoop));
    }

    private void StartTitleWaveLoop()
    {
        foreach (var t in waveTweens) { if (t != null && t.IsValid()) t.Kill(); }
        waveTweens.Clear();

        for (int i = 0; i < generatedLetterLabels.Count; i++)
        {
            Label lLabel = generatedLetterLabels[i];
            float lDelayOffset = i * waveFrequencyDelay;

            Tween lWaveTween = CreateTween();
            waveTweens.Add(lWaveTween);

            lWaveTween.SetLoops();

            lWaveTween.TweenProperty(lLabel, (string)PropertyName.Position + ":y", -waveAmplitude, waveDuration * 0.5f)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.InOut)
                .SetDelay(lDelayOffset);

            lWaveTween.Parallel().TweenProperty(lLabel, (string)PropertyName.RotationDegrees, rotationAmplitude, waveDuration * 0.5f)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.InOut)
                .SetDelay(lDelayOffset);

            lWaveTween.Chain().TweenProperty(lLabel, (string)PropertyName.Position + ":y", waveAmplitude, waveDuration * 0.5f)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.InOut);

            lWaveTween.Parallel().TweenProperty(lLabel, (string)PropertyName.RotationDegrees, -rotationAmplitude, waveDuration * 0.5f)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.InOut);
        }
    }
}
