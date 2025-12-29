using Godot;
using System.Collections.Generic;

public partial class MainMenu : Control
{
    [ExportCategory("References")]
    [Export] private Button startButton;
    [Export] private Button optionButton;
    [Export] private Button creditsButton;
    [Export] private Button quitButton;
    [Export] private Button franceButton;
    [Export] private Button englishButton;
    [Export] private Button backButton;
    [Export] private Control pivotPoint;
    [Export] private Label originalLabel;

    [ExportCategory("Audio")]
    [Export] private AudioStream hoverSound;
    [Export] private AudioStream clickSound;

    [ExportCategory("Animation Settings")]
    [Export] private float letterSpacing = 1f;
    [Export] private float animSpeed = 0.08f;

    [ExportGroup("Wave Effect")]
    [Export] private float waveAmplitude = 4f;
    [Export] private float waveSpeed = 2.5f;
    [Export] private float waveFrequency = 0.4f;
    [Export] private float rotAmplitude = 3f;

    [ExportGroup("Button Feel")]
    [Export] private float hoverScale = 1.1f;
    [Export] private Color hoverColor = new Color(1.2f, 1.2f, 1.2f);

    private readonly List<Label> letterLabels = new();
    private List<Button> allButtons = new();
    private bool isFlipped;
    private float currentWaveIntensity = 0f;
    private double timePassed = 0;
    private Tween flipTween;

    private bool isLocked = true;

    public override void _Ready()
    {
        allButtons = new List<Button> { startButton, optionButton, creditsButton, quitButton, franceButton, englishButton, backButton };

        SetupConnections();
        SetupButtonVisuals();
        PrepareText();
        AnimateIntroText();
        AnimateButtonsEntry();

        // Note : On ne grab pas le focus tout de suite ici, on attend la fin de l'anim
    }

    public override void _Process(double delta)
    {
        timePassed += delta;

        for (int i = 0; i < letterLabels.Count; i++)
        {
            Label l = letterLabels[i];
            float wavePhase = (float)timePassed * waveSpeed + i * waveFrequency;

            float y = Mathf.Sin(wavePhase) * waveAmplitude * currentWaveIntensity;
            float r = Mathf.Cos(wavePhase * 0.5f) * rotAmplitude * currentWaveIntensity;

            l.Position = new Vector2(0, y);
            l.RotationDegrees = r;
        }
    }

    private void SetupConnections()
    {
        startButton.Pressed += OnStart;
        optionButton.Pressed += OnOption;
        creditsButton.Pressed += OnCredits;
        quitButton.Pressed += OnQuit;
        franceButton.Pressed += OnFrance;
        englishButton.Pressed += OnEnglish;
        backButton.Pressed += OnBack;

        foreach (var btn in allButtons)
        {
            btn.MouseEntered += () => OnButtonHover(btn, true);
            btn.MouseExited += () => OnButtonHover(btn, false);
            btn.FocusEntered += () => OnButtonHover(btn, true);
            btn.FocusExited += () => OnButtonHover(btn, false);
        }
    }

    private void SetupButtonVisuals()
    {
        foreach (var btn in allButtons)
        {
            btn.PivotOffset = btn.Size / 2;
        }
    }

    private void OnButtonHover(Button btn, bool isHovered)
    {
        Tween t = CreateTween();
        t.SetParallel(true);
        t.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);

        Vector2 targetScale = isHovered ? new Vector2(hoverScale, hoverScale) : Vector2.One;
        Color targetColor = isHovered ? hoverColor : Colors.White;

        t.TweenProperty(btn, "scale", targetScale, 0.4f);
        t.TweenProperty(btn, "modulate", targetColor, 0.2f).SetTrans(Tween.TransitionType.Sine);
    }

    private void OnStart()
    {
        if (isLocked) return;
        isLocked = true;

        SceneManager.GetInstance().SwitchScene(SceneManager.SceneType.Game);
    }

    private void OnCredits()
    {
        if (isLocked) return;
        // Logique crédits...
    }

    private void OnQuit()
    {
        if (isLocked) return;
        isLocked = true; // On verrouille pour éviter de spammer Quit pendant la fermeture
        GetTree().Quit();
    }

    private void OnFrance()
    {
        // Pas besoin de bloquer ici sauf si ça lag, c'est instantané
        TranslationServer.SetLocale("fr");
    }

    private void OnEnglish()
    {
        TranslationServer.SetLocale("en");
    }

    private void OnOption()
    {
        if (isLocked) return;
        FlipMenu(true);
    }

    private void OnBack()
    {
        if (isLocked) return;
        FlipMenu(false);
    }

    private void FlipMenu(bool showOptions)
    {
        isLocked = true;

        isFlipped = showOptions;
        float targetAngle = isFlipped ? 180f : 0f;

        flipTween?.Kill();
        flipTween = CreateTween();
        flipTween.SetParallel(true);

        flipTween.TweenProperty(pivotPoint, "rotation_degrees", targetAngle, 0.6f)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Back);

        var depthTween = CreateTween();
        depthTween.TweenProperty(pivotPoint, "scale", new Vector2(0.9f, 0.9f), 0.3f)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        depthTween.TweenProperty(pivotPoint, "scale", Vector2.One, 0.3f)
            .SetDelay(0.3f)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);

        flipTween.Finished += () =>
        {
            isLocked = false;
            if (showOptions) backButton.GrabFocus();
            else optionButton.GrabFocus();
        };
    }

    private void AnimateIntroText()
    {
        Tween t = CreateTween();
        t.SetParallel(true);

        for (int i = 0; i < letterLabels.Count; i++)
        {
            float d = i * animSpeed;
            Label l = letterLabels[i];

            l.Scale = Vector2.Zero;
            l.Modulate = new Color(1, 1, 1, 0);

            t.TweenProperty(l, "scale", Vector2.One, 0.4f).SetDelay(d).SetTrans(Tween.TransitionType.Back);
            t.TweenProperty(l, "modulate:a", 1f, 0.3f).SetDelay(d);
        }

        Tween waveT = CreateTween();
        waveT.TweenMethod(Callable.From<float>(v => currentWaveIntensity = v), 0f, 1f, 2.5f)
             .SetDelay(0.2f)
             .SetEase(Tween.EaseType.Out);
    }

    private void AnimateButtonsEntry()
    {
        var mainButtons = new List<Control> { startButton, optionButton, creditsButton, quitButton };

        Tween t = CreateTween();
        t.SetParallel(true);

        for (int i = 0; i < mainButtons.Count; i++)
        {
            Control btn = mainButtons[i];
            btn.Modulate = new Color(1, 1, 1, 0);
            btn.Scale = new Vector2(0.5f, 0.5f);

            float delay = i * 0.1f + 0.5f;

            t.TweenProperty(btn, "modulate:a", 1f, 0.3f).SetDelay(delay);
            t.TweenProperty(btn, "scale", Vector2.One, 0.6f)
                .SetDelay(delay)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Elastic);
        }

        t.Finished += () =>
        {
            isLocked = false;
            startButton.GrabFocus();
        };
    }

    private void PrepareText()
    {
        string text = originalLabel.Text;
        originalLabel.Visible = false;

        HBoxContainer container = new();
        container.Alignment = BoxContainer.AlignmentMode.Center;
        container.Position = originalLabel.Position;
        container.Size = originalLabel.Size;
        container.MouseFilter = MouseFilterEnum.Ignore;

        originalLabel.GetParent().AddChild(container);

        foreach (char c in text)
        {
            Control wrap = new();
            Label l = new();

            l.Text = c.ToString();
            l.LabelSettings = originalLabel.LabelSettings;
            l.Modulate = new Color(1, 1, 1, 0);

            Vector2 charSize = l.GetThemeFont("font").GetStringSize(l.Text, l.HorizontalAlignment, -1, l.LabelSettings.FontSize);
            l.PivotOffset = new Vector2(charSize.X / 2, l.LabelSettings.FontSize / 2);

            wrap.AddChild(l);
            container.AddChild(wrap);

            Vector2 size = charSize * letterSpacing;
            if (string.IsNullOrWhiteSpace(l.Text)) size.X = l.LabelSettings.FontSize / 3;

            wrap.CustomMinimumSize = size;
            letterLabels.Add(l);
        }
    }
}