using Godot;
using System;

using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NToolsBox;
using Com.IsartDigital.Shmup.NStats;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

namespace Com.IsartDigital.Shmup.NHUD;

public partial class HUD : MenuBase
{
    [Export] private PackedScene bubbleScore;

    [ExportCategory("References")]
    [Export] public HBoxContainer healthContainer;
    [Export] public HBoxContainer powContainer;
    [Export] public Label scoreLabel;
    [Export] public Label godmodeLabel;
    [Export] public Label gameoverLabel;
    [Export] private Label bossLabel;
    [Export] private ProgressBar bossTextureBar;

    [ExportCategory("Colors")]
    [Export] public Color activeColor = Colors.White;
    [Export] public Color inactiveColor = new Color(1, 1, 1, 0.2f);
    [Export] public Color bossDamageColor = new Color(1, 0.2f, 0.2f, 1.0f);
    [Export] public Color invisibleColor = Colors.Transparent;

    [ExportCategory("Score")]
    [Export] public float popDuration = 0.25f;
    [Export] public float popScaleFactor = 1.5f;
    [Export] public float scoreCountDuration = 0.5f;

    [ExportCategory("Entrance")]
    [Export] public float entranceOffsetY = 50f;
    [Export] public float entranceFadeDuration = 0.5f;
    [Export] public float entranceMoveDuration = 0.6f;
    [Export] public float entranceElementScaleDuration = 0.4f;
    [Export] public float entranceHealthDelay = 0.1f;
    [Export] public float entranceScoreDelay = 0.2f;
    [Export] public float entrancePowDelay = 0.3f;

    [ExportCategory("Exit")]
    [Export] public float exitOffsetY = 150f;
    [Export] public float exitModulateMult = 0.8f;

    [ExportCategory("Boss")]
    [Export] public float bossFadeDuration = 0.5f;
    [Export] public float bossFillDuration = 1.2f;
    [Export] public float bossHitDuration = 0.4f;
    [Export] public float bossColorReturnDuration = 0.3f;
    private bool isBossActive = false;

    [ExportCategory("Game Over")]
    [Export] public float gameOverFallDuration = 1.0f;
    [Export] public float gameOverStartOffsetY = 100f;

    private Vector2 originalScale;
    private float originalY;
    private float bossBarOriginalX;
    private float gameOverOriginalY;

    private const float ALPHA_TRANSPARENT = 0f;
    private const float ALPHA_OPAQUE = 1f;

    private const float DEFAULT_BOSS_VAL = 0f;

    private static HUD instance;
    public static HUD GetInstance()
    {
        return instance;
    }

    // ========================================================================
    // INIT
    // ========================================================================

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

        bossBarOriginalX = bossTextureBar.Position.X;
        bossTextureBar.Modulate = invisibleColor;

        bossLabel.Modulate = invisibleColor;

        gameOverOriginalY = gameoverLabel.Position.Y;
        gameoverLabel.Visible = false;

        Visible = false;
    }

    public BubbleScore CreateBubbleScore(Vector2 pPos, int pScore, Color pColor)
    {
        BubbleScore lBubbleScore = (BubbleScore)bubbleScore.Instantiate();
        lBubbleScore.Position = pPos;
        lBubbleScore.score = pScore;

        if (lBubbleScore.label.LabelSettings != null)
        {
            lBubbleScore.label.LabelSettings = (LabelSettings)lBubbleScore.label.LabelSettings.Duplicate();
            lBubbleScore.label.LabelSettings.FontColor = pColor;
        }

        GameManager.GetInstance().gameContainer.AddChild(lBubbleScore);

        return lBubbleScore;
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    public override void OnEnter()
    {
        Visible = true;
        gameoverLabel.Visible = false;

        PlayEntranceAnimation();

        if (isBossActive)
        {
            bossTextureBar.Modulate = Colors.White;
            bossLabel.Modulate = Colors.White;
        }
        else
            HideBossBattle();

        AudioManager.GetInstance().SetMusicPaused(false);
    }

    public override void OnExit()
    {
        base.OnExit();
        PlayExitAnimation(animDurationExit);
    }

    private void SetScoreText(int pValue)
    {
        scoreLabel.Text = pValue.ToString();
    }

    public void UpdateHealth(int pCurrentHealth)
    {
        UpdateContainerAlpha(healthContainer, pCurrentHealth);
    }

    public void UpdatePow(int pCurrentPow)
    {
        UpdateContainerAlpha(powContainer, pCurrentPow);
    }

    // ========================================================================
    // ANIMATION BOSS
    // ========================================================================

    public void SetupBossBattle(int pMaxHealth)
    {
        isBossActive = true;

        bossTextureBar.MaxValue = pMaxHealth;
        bossTextureBar.Value = DEFAULT_BOSS_VAL;

        Tween lTween = CreateTween();

        lTween.TweenProperty(bossTextureBar, (string)PropertyName.Modulate + ":a", ALPHA_OPAQUE, bossFadeDuration);
        lTween.Parallel();

        lTween.TweenProperty(bossLabel, (string)PropertyName.Modulate + ":a", ALPHA_OPAQUE, bossFadeDuration);

        lTween.Chain().TweenProperty(bossTextureBar, (string)TextureProgressBar.PropertyName.Value, pMaxHealth, bossFillDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }

    public void ShowBossBattle()
    {
        Tween lTween = CreateTween();
        lTween.SetParallel(true);

        bossTextureBar.Value = DEFAULT_BOSS_VAL;

        lTween.TweenProperty(bossTextureBar, (string)CanvasItem.PropertyName.Modulate + ":a", ALPHA_OPAQUE, bossFadeDuration);
        lTween.TweenProperty(bossLabel, (string)CanvasItem.PropertyName.Modulate + ":a", ALPHA_OPAQUE, bossFadeDuration);
    }


    public void HideBossBattle()
    {
        isBossActive = false;

        bossTextureBar.Modulate = invisibleColor;
        bossTextureBar.Value = DEFAULT_BOSS_VAL;
        bossLabel.Modulate = invisibleColor;
    }

    public void UpdateBossHealth(int pCurrentHealth)
    {
        if (bossTextureBar == null) return;

        Tween lTween = CreateTween();
        lTween.SetParallel(true);

        lTween.TweenProperty(bossTextureBar, (string)ProgressBar.PropertyName.Value, pCurrentHealth, bossHitDuration)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);

        bossTextureBar.Modulate = bossDamageColor;

        lTween.TweenProperty(bossTextureBar, (string)CanvasItem.PropertyName.Modulate, Colors.White, bossColorReturnDuration);

        Effect.BumpHUD(bossTextureBar, originalScale);
    }

    public void ResetHUD()
    {
        isBossActive = false;
        HideBossBattle();

        scoreLabel.Text = "0";
        scoreLabel.Scale = originalScale;
    }

    // ========================================================================
    // ANIMATION HUD
    // ========================================================================

    public void AnimatedScore(int pNewScore)
    {
        int lCurrentScore = GameStats.currentScore;
        int lTargetScore = lCurrentScore + pNewScore;

        GameStats.currentScore = lTargetScore;

        Tween lTween = CreateTween();
        lTween.SetParallel(true);

        lTween.TweenMethod(Callable.From<int>(SetScoreText), lCurrentScore, lTargetScore, scoreCountDuration)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);

        lTween.TweenProperty(scoreLabel, (string)PropertyName.Scale, originalScale * popScaleFactor, popDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        lTween.Chain().TweenProperty(scoreLabel, (string)PropertyName.Scale, originalScale, popDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);
    }

    public void PlayEntranceAnimation()
    {
        Modulate = Colors.Transparent;
        Position = new Vector2(Position.X, originalY - entranceOffsetY);

        healthContainer.Scale = Vector2.Zero;
        powContainer.Scale = Vector2.Zero;
        scoreLabel.Scale = Vector2.Zero;

        Tween lTween = CreateTween();
        lTween.SetParallel(true);

        lTween.TweenProperty(this, (string)CanvasItem.PropertyName.Modulate + ":a", ALPHA_OPAQUE, entranceFadeDuration);
        lTween.TweenProperty(this, (string)Node2D.PropertyName.Position + ":y", originalY, entranceMoveDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        if (healthContainer != null)
            lTween.TweenProperty(healthContainer, (string)Node2D.PropertyName.Scale, Vector2.One, entranceElementScaleDuration)
                .SetDelay(entranceHealthDelay)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);

        if (scoreLabel != null)
            lTween.TweenProperty(scoreLabel, (string)Node2D.PropertyName.Scale, originalScale, entranceElementScaleDuration)
                .SetDelay(entranceScoreDelay)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);

        if (powContainer != null)
            lTween.TweenProperty(powContainer, (string)Node2D.PropertyName.Scale, Vector2.One, entranceElementScaleDuration)
                .SetDelay(entrancePowDelay)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);

        lTween.Finished += () => MenuManager.GetInstance().loading = false;
    }

    public void PlayExitAnimation(float pDuration)
    {
        Tween lTween = CreateTween();
        lTween.SetParallel(true);

        lTween.TweenProperty(this, (string)Node2D.PropertyName.Position + ":y", originalY - exitOffsetY, pDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);

        lTween.TweenProperty(this, (string)CanvasItem.PropertyName.Modulate + ":a", ALPHA_TRANSPARENT, pDuration * exitModulateMult);

        lTween.Chain().TweenCallback(Callable.From(() => Visible = false));
    }

    private void UpdateContainerAlpha(HBoxContainer pContainer, int pValue)
    {
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

    // ========================================================================
    // ANIMATION GAME OVER
    // ========================================================================

    public void PlayGameOver()
    {
        gameoverLabel.Visible = true;
        gameoverLabel.Modulate = Colors.White;

        float lStartY = -gameoverLabel.Size.Y - gameOverStartOffsetY;
        gameoverLabel.Position = new Vector2(gameoverLabel.Position.X, lStartY);

        Tween lTween = CreateTween();

        lTween.TweenProperty(gameoverLabel, (string)Node2D.PropertyName.Position + ":y", gameOverOriginalY, gameOverFallDuration)
              .SetTrans(Tween.TransitionType.Bounce)
              .SetEase(Tween.EaseType.Out);
    }

    // ========================================================================
    // ANIMATION OTHER
    // ========================================================================

    public void SetGodMode(bool pEnabled)
    {
        if (godmodeLabel != null) godmodeLabel.Visible = pEnabled;
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}