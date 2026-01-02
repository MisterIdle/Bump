using Godot;
using System;

public partial class HUD : Control
{
    private static HUD instance;
    public static HUD GetInstance()
    {
        return instance;
    }

    [Export] private PackedScene bubbleScore;

    [ExportCategory("References")]
    [Export] public HBoxContainer HealthContainer;
    [Export] public HBoxContainer PowContainer;
    [Export] public Label scoreLabel;
    [Export] public Label godmodeLabel;

    [ExportCategory("Colors")]
    [Export] public Color activeColor = new Color(1, 1, 1, 1.0f);
    [Export] public Color inactiveColor = new Color(1, 1, 1, 0.2f);
    [Export] public Color bossDamageColor = new Color(1, 0.2f, 0.2f, 1.0f);

    [ExportCategory("Animation Settings")]
    [Export(PropertyHint.Range, "0.1, 2.0")]
    public float popDuration = 0.25f;

    [Export(PropertyHint.Range, "1.0, 3.0")]
    public float popScaleFactor = 1.5f;

    [Export(PropertyHint.Range, "0.1, 5.0")]
    public float scoreCountDuration = 0.5f;

    [ExportCategory("Boss UI")]
    [Export] private Label bossLabel;
    [Export] private ProgressBar bossTextureBar;

    private Vector2 originalScale;
    private float originalY;
    private float bossBarOriginalX;

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else
        {
            QueueFree();
            return;
        }

        originalScale = scoreLabel.Scale;
        originalY = Position.Y;

        // On sauvegarde la position X de la barre du boss pour le tremblement
        if (bossTextureBar != null)
        {
            bossBarOriginalX = bossTextureBar.Position.X;
            bossTextureBar.Modulate = new Color(1, 1, 1, 0);
        }

        if (bossLabel != null)
        {
            bossLabel.Modulate = new Color(1, 1, 1, 0);
        }

        PlayEntranceAnimation();
    }

    public void SetupBossBattle(int pMaxHealth)
    {
        if (bossTextureBar == null) return;

        bossTextureBar.MaxValue = pMaxHealth;
        bossTextureBar.Value = 0;

        Tween lTween = CreateTween();
        
        lTween.TweenProperty(bossTextureBar, "modulate:a", 1.0f, 0.5f);
        
        if (bossLabel != null)
        {
            lTween.Parallel().TweenProperty(bossLabel, "modulate:a", 1.0f, 0.5f);
        }

        lTween.Chain().TweenProperty(bossTextureBar, "value", pMaxHealth, 1.2f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }

    public void UpdateBossHealth(int pCurrentHealth)
    {
        if (bossTextureBar == null) return;

        Tween lTween = CreateTween();
        lTween.SetParallel(true);

        lTween.TweenProperty(bossTextureBar, "value", pCurrentHealth, 0.4f)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);

        bossTextureBar.Modulate = bossDamageColor;
        lTween.TweenProperty(bossTextureBar, "modulate", new Color(1, 1, 1, 1), 0.3f);

        Effect.BumpHUD(bossTextureBar, originalScale);
    }

    public BubbleScore CreateBubbleScore(Vector2 pPos, int pScore)
    {
        BubbleScore lBubbleScore = (BubbleScore)bubbleScore.Instantiate();
        lBubbleScore.Position = pPos;
        lBubbleScore.score = pScore;

        GameManager.GetInstance().gameContainer.AddChild(lBubbleScore);

        return lBubbleScore;
    }

    public void AnimatedScore(int pNewScore)
    {
        int lCurrentScore = GameManager.GetInstance().score;
        int lTargetScore = lCurrentScore + pNewScore;

        GameManager.GetInstance().score = lTargetScore;

        Tween lTween = CreateTween();
        lTween.SetParallel(true);

        lTween.TweenMethod(Callable.From<int>(SetScoreText), lCurrentScore, lTargetScore, scoreCountDuration)
              .SetEase(Tween.EaseType.Out)
              .SetTrans(Tween.TransitionType.Cubic);

        lTween.TweenProperty(scoreLabel, "scale", originalScale * popScaleFactor, popDuration)
              .SetTrans(Tween.TransitionType.Back)
              .SetEase(Tween.EaseType.Out);

        lTween.Chain().TweenProperty(scoreLabel, "scale", originalScale, popDuration)
              .SetTrans(Tween.TransitionType.Back)
              .SetEase(Tween.EaseType.In);
    }

    public void PlayEntranceAnimation()
    {
        Modulate = new Color(1, 1, 1, 0); 
        Position = new Vector2(Position.X, originalY - 50);

        HealthContainer.Scale = Vector2.Zero;
        PowContainer.Scale = Vector2.Zero;
        scoreLabel.Scale = Vector2.Zero;

        Tween lTween = CreateTween();
        lTween.SetParallel(true);

        lTween.TweenProperty(this, "modulate:a", 1f, 0.5f);
        lTween.TweenProperty(this, "position:y", originalY, 0.6f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        lTween.TweenProperty(HealthContainer, "scale", Vector2.One, 0.4f)
            .SetDelay(0.1f)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);

        lTween.TweenProperty(scoreLabel, "scale", originalScale, 0.4f)
            .SetDelay(0.2f)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);

        lTween.TweenProperty(PowContainer, "scale", Vector2.One, 0.4f)
            .SetDelay(0.3f)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
    }

    public void PlayExitAnimation(float pDuration)
    {
        Tween lTween = CreateTween();
        lTween.SetParallel(true);

        lTween.TweenProperty(this, "position:y", originalY - 150, pDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);

        lTween.TweenProperty(this, "modulate:a", 0f, pDuration * 0.8f);
    }

    private void SetScoreText(int pValue)
    {
        scoreLabel.Text = pValue.ToString();
    }

    public void UpdateHealth(int pCurrentHealth)
    {
        UpdateContainerAlpha(HealthContainer, pCurrentHealth);
    }

    public void UpdatePow(int pCurrentPow)
    {
        UpdateContainerAlpha(PowContainer, pCurrentPow);
    }

    public void SetGodMode(bool pEnabled)
    {
        godmodeLabel.Visible = pEnabled;
    }

    private void UpdateContainerAlpha(HBoxContainer pContainer, int pValue)
    {
        if (pContainer == null) return;

        int lIndex = 0;

        foreach (Node lNode in pContainer.GetChildren())
        {
            if (lNode is TextureRect lTextureRect)
            {
                lTextureRect.Modulate = lIndex < pValue ? activeColor : inactiveColor;

                Effect.BumpHUD(lTextureRect, Scale);
            }
            lIndex++;
        }
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}