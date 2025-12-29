
using Godot;
using System;

public partial class HUD : Control
{
    private static HUD instance;

    [Export] private PackedScene bubbleScore;

    [ExportCategory("References")]
    [Export] public HBoxContainer HealthContainer;
    [Export] public HBoxContainer PowContainer;
    [Export] public Label scoreLabel;
    [Export] public Label godmodeLabel;

    [ExportCategory("Colors")]
    [Export] public Color activeColor = new Color(1, 1, 1, 1.0f);
    [Export] public Color inactiveColor = new Color(1, 1, 1, 0.2f);

    [ExportCategory("Animation Settings")]
    [Export(PropertyHint.Range, "0.1, 2.0")]
    public float popDuration = 0.25f;

    [Export(PropertyHint.Range, "1.0, 3.0")]
    public float popScaleFactor = 1.5f;

    [Export(PropertyHint.Range, "0.1, 5.0")]
    public float scoreCountDuration = 0.5f;

    private Vector2 originalScale;

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else
        {
            QueueFree();
            return;
        }

 
        originalScale = scoreLabel.Scale;
    }

    public static HUD GetInstance()
    {
        return instance;
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
}