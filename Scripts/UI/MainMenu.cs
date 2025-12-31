using Godot;
using System.Collections.Generic;

public partial class MainMenu : Control
{
    [ExportCategory("References")]
    [Export] private MenuManager menuManager;
    [Export] private Label Title;

    [ExportSubgroup("Buttons")]
    [Export] private Button startButton;
    [Export] private Button optionButton;
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

    public override void _Ready()
    {
        PrepareTitleText();
        PlayButtonIntroAnimation();
        PlayTitleIntroAnimation();

        optionButton.Pressed += OnOptionPressed;
        startButton.Pressed += OnStartPressed;
        quitButton.Pressed += OnQuitPressed;
    }

    private void OnOptionPressed()
    {
        menuManager.ToggleMenu(MenuManager.MenuContext.MainMenu);
    }

    private void OnStartPressed()
    {
        SceneManager.GetInstance().SwitchScene(SceneManager.SceneType.Game);
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }

    private void PrepareTitleText()
    {
        string lContent = Title.Text;
        Title.Visible = false;

        string lFont = "font";

        HBoxContainer lContainer = new HBoxContainer();
        lContainer.Alignment = BoxContainer.AlignmentMode.Center;
        lContainer.Position = Title.Position;
        lContainer.Size = Title.Size;

        Title.GetParent().CallDeferred(Node.MethodName.AddChild, lContainer);

        foreach (char lLetter in lContent)
        {
            Control lWrapper = new Control();
            Label lLetterLabel = new Label();

            lLetterLabel.Text = lLetter.ToString();
            lLetterLabel.LabelSettings = Title.LabelSettings;
            lLetterLabel.Modulate = new Color(1, 1, 1, 0);

            Vector2 lCharSize = lLetterLabel.GetThemeFont(lFont).GetStringSize(lLetterLabel.Text, lLetterLabel.HorizontalAlignment, -1, lLetterLabel.LabelSettings.FontSize);

            lLetterLabel.PivotOffset = new Vector2(lCharSize.X / 2, lLetterLabel.LabelSettings.FontSize / 2);

            lWrapper.AddChild(lLetterLabel);
            lContainer.AddChild(lWrapper);

            Vector2 lWrapperSize = lCharSize * letterSpacing;

            lWrapper.CustomMinimumSize = lWrapperSize;
            generatedLetterLabels.Add(lLetterLabel);
        }
    }

    private void PlayTitleIntroAnimation()
    {
        Tween lTween = CreateTween();
        lTween.SetParallel(true);

        for (int i = 0; i < generatedLetterLabels.Count; i++)
        {
            Label lLabel = generatedLetterLabels[i];
            float lDelay = i * introLetterSpeed;

            lLabel.Scale = Vector2.Zero;
            lLabel.Modulate = new Color(1, 1, 1, 0);

            lTween.TweenProperty(lLabel, (string)PropertyName.Scale, Vector2.One, 0.4f)
                .SetDelay(lDelay)
                .SetTrans(Tween.TransitionType.Back);

            lTween.TweenProperty(lLabel, (string)PropertyName.Modulate + ":a", 1f, 0.3f)
                .SetDelay(lDelay);
        }

        lTween.Chain().TweenCallback(Callable.From(StartTitleWaveLoop));
    }

    private void StartTitleWaveLoop()
    {
        for (int i = 0; i < generatedLetterLabels.Count; i++)
        {
            Label lLabel = generatedLetterLabels[i];
            float lDelayOffset = i * waveFrequencyDelay;

            Tween lWaveTween = CreateTween();
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

    private void PlayButtonIntroAnimation()
    {
        Tween lButtonTween = CreateTween();
        lButtonTween.SetParallel(true);
        float lCurrentDelay = 0.5f;

        AnimateButtonsRecursive(this, lButtonTween, ref lCurrentDelay);

        lButtonTween.Finished += () => startButton.GrabFocus();
    }

    private void AnimateButtonsRecursive(Node pNode, Tween pTween, ref float pDelay)
    {
        foreach (Node lChild in pNode.GetChildren())
        {
            if (lChild is Button lButton)
            {
                lButton.Modulate = new Color(1, 1, 1, 0);
                lButton.Scale = new Vector2(0.5f, 0.5f);
                lButton.PivotOffset = lButton.Size / 2;

                pTween.TweenProperty(lButton, (string)PropertyName.Modulate + ":a", 1f, 0.3f)
                    .SetDelay(pDelay);

                pTween.TweenProperty(lButton, (string)PropertyName.Scale, Vector2.One, 0.6f)
                    .SetDelay(pDelay)
                    .SetEase(Tween.EaseType.Out)
                    .SetTrans(Tween.TransitionType.Elastic);

                pDelay += 0.1f;
            }

            if (lChild.GetChildCount() > 0)
            {
                AnimateButtonsRecursive(lChild, pTween, ref pDelay);
            }
        }
    }
}