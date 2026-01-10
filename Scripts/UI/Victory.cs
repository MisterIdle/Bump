using Godot;
using System;

using Com.IsartDigital.Shmup.NStats;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;

namespace Com.IsartDigital.Shmup.NHUD;

public partial class Victory : MenuBase
{
    [ExportCategory("UI References")]
    [Export] private Label titleLabel;

    [ExportGroup("Stats Labels")]
    [Export] private Label killCountText;
    [Export] private Label killCountLabel;
    [Export] private Label shotCountText;
    [Export] private Label shotCountLabel;
    [Export] private Label specialCountText;
    [Export] private Label specialCountLabel;
    [Export] private Label powCountText;
    [Export] private Label powCountLabel;

    [ExportGroup("Score & Rank")]
    [Export] private Label scoreText;
    [Export] private Label scoreLabel;
    [Export] private Label rankLabel;

    [ExportCategory("Data Setup")]
    [Export] private int[] rankScoreThresholds;
    [Export] private string[] rankTitles;
    [Export] private Color[] rankColors;

    [ExportCategory("Animation Timing")]
    [Export] private float initialDelay = 0.5f;
    [Export] private float stepDelay = 0.3f;
    [Export] private float titleFadeDuration = 0.5f;
    [Export] private float statFadeDuration = 0.2f;
    [Export] private float statCountDuration = 0.5f;

    [ExportGroup("Score Dynamic Timing")]
    [Export] private float scoreMinDuration = 0.5f;
    [Export] private float scoreMaxDuration = 3.0f;
    [Export] private int scoreForMaxDuration = 999999999;

    [ExportCategory("Animation Scale & Juice")]
    [Export] private float statPopMultiplier = 1.1f;
    [Export] private float rankStartScaleMultiplier = 3.0f;
    [Export] private float rankStampDuration = 0.5f;
    [Export] private float shakeStrength = 10.0f;

    [ExportGroup("Score Heat Effect")]
    [Export] private Color scoreHeatColor = Colors.Red;
    [Export] private float scoreHeatScaleMultiplier = 1.1f;
    [Export] private float scoreCoolDownDuration = 0.4f;

    private Vector2 baseScaleTitle;

    private Vector2 baseScaleKill;
    private Vector2 baseScaleShot;
    private Vector2 baseScaleSpecial;
    private Vector2 baseScalePow;

    private Vector2 baseScaleKillText;
    private Vector2 baseScaleShotText;
    private Vector2 baseScaleSpecialText;
    private Vector2 baseScalePowText;

    private Vector2 baseScaleScoreText;
    private Vector2 baseScaleScore;

    private Vector2 baseScaleRank;


    private const float ALPHA_TRANSPARENT = 0f;
    private const float ALPHA_OPAQUE = 1f;

    private const float PIVOT_CENTER_RATIO = 0.5f;
    private const float HALF_MULTIPLIER = 0.5f;

    private const float SCALE_HIDDEN_MULT = 0.8f;

    private const float RANK_SCALE_HUGE_MULT = 5.0f;
    private const float RANK_SCALE_SLAM_MULT = 0.9f;
    private const float RANK_ROTATION_START = -15.0f;
    private const float RANK_ROTATION_SLAM = -2.0f;

    private const float ANIM_SLAM_FADE_DURATION = 0.1f;
    private const float ANIM_SLAM_DURATION = 0.25f;
    private const float ANIM_SHAKE_STEP_DURATION = 0.05f;

    private const float STAT_POP_UP_RATIO = 0.4f;
    private const float STAT_POP_DOWN_RATIO = 0.6f;
    private const float STAT_FLASH_IN_DURATION = 0.1f;
    private const float STAT_FLASH_OUT_DURATION = 0.15f;

    private const float COLOR_OVERBRIGHT_VAL = 1.2f;

    private const int DEFAULT_SCORE_MIN = 1;
    private const int START_VALUE_ZERO = 0;
    private const float CLAMP_MIN = 0f;
    private const float CLAMP_MAX = 1f;

    private const string DEFAULT_RANK_TITLE = "NULL";
    private const string EMPTY_STRING = "";

    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        baseScaleTitle = titleLabel.Scale;

        baseScaleKill = killCountLabel.Scale;
        baseScaleShot = shotCountLabel.Scale;
        baseScaleSpecial = specialCountLabel.Scale;
        baseScalePow = powCountLabel.Scale;

        baseScaleKillText = killCountText.Scale;
        baseScaleShotText = shotCountText.Scale;
        baseScaleSpecialText = specialCountText.Scale;
        baseScalePowText = powCountText.Scale;

        baseScaleScoreText = scoreText.Scale;
        baseScaleScore = scoreLabel.Scale;

        baseScaleRank = rankLabel.Scale;

        HideAllElements();
    }

    // ========================================================================
    // INPUT
    // ========================================================================

    public override void _Input(InputEvent pEvent)
    {
        if (!Visible) return;

        bool lIsAnyAction = pEvent.IsPressed() &&
                           (pEvent is InputEventKey ||
                            pEvent is InputEventMouseButton ||
                            pEvent is InputEventJoypadButton);

        if (lIsAnyAction && !MenuManager.GetInstance().loading)
        {
            GetViewport().SetInputAsHandled();
            PlayGenericExitAnimation();

            MenuManager.GetInstance().SetMenu(MenuManager.MenuState.MainMenu);
            SceneManager.GetInstance().ChangeGlobalScene(SceneManager.SceneType.MainMenu);
        }
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    public override void OnEnter()
    {
        Visible = true;

        HideAllElements();
        PlayVictory();

        Player lPlayer = Player.GetInstance();
        if (lPlayer != null) lPlayer.invincible = true;
    }

    // ========================================================================
    // ANIMATION RESET
    // ========================================================================

    private void HideAllElements()
    {
        ResetLabelVisuals(titleLabel, baseScaleTitle, false);

        ResetLabelVisuals(killCountLabel, baseScaleKill, true);
        ResetLabelVisuals(shotCountLabel, baseScaleShot, true);
        ResetLabelVisuals(specialCountLabel, baseScaleSpecial, true);
        ResetLabelVisuals(powCountLabel, baseScalePow, true);

        ResetLabelVisuals(killCountText, baseScaleKillText, false);
        ResetLabelVisuals(shotCountText, baseScaleShotText, false);
        ResetLabelVisuals(specialCountText, baseScaleSpecialText, false);
        ResetLabelVisuals(powCountText, baseScalePowText, false);

        ResetLabelVisuals(scoreText, baseScaleScoreText, false);
        ResetLabelVisuals(scoreLabel, baseScaleScore, true);

        if (rankLabel != null)
        {
            rankLabel.Modulate = Colors.Transparent;
            rankLabel.Text = EMPTY_STRING;
            rankLabel.Scale = new Vector2(rankStartScaleMultiplier, rankStartScaleMultiplier);
            rankLabel.PivotOffset = rankLabel.Size * PIVOT_CENTER_RATIO;
        }
    }

    private void ResetLabelVisuals(Label pLabel, Vector2 pBaseScale, bool pResetText)
    {
        if (pLabel == null) return;

        pLabel.Modulate = Colors.Transparent;
        pLabel.Scale = pBaseScale;

        if (pResetText)
        {
            pLabel.PivotOffset = pLabel.Size * PIVOT_CENTER_RATIO;
        }
    }

    // ========================================================================
    // ANIMATION GLOBAL
    // ========================================================================

    private void PlayVictory()
    {
        Tween lVictoryTween = CreateTween();

        lVictoryTween.TweenInterval(initialDelay);

        lVictoryTween.TweenProperty(titleLabel, (string)CanvasItem.PropertyName.Modulate + ":a", ALPHA_OPAQUE, titleFadeDuration);
        lVictoryTween.TweenInterval(stepDelay);

        AddStatToSequence(lVictoryTween, killCountText, killCountLabel, GameStats.enemyKills, baseScaleKill);
        AddStatToSequence(lVictoryTween, shotCountText, shotCountLabel, GameStats.shotsFired, baseScaleShot);
        AddStatToSequence(lVictoryTween, specialCountText, specialCountLabel, GameStats.specialUsed, baseScaleSpecial);
        AddStatToSequence(lVictoryTween, powCountText, powCountLabel, GameStats.powerUsed, baseScalePow);

        lVictoryTween.TweenInterval(stepDelay);

        lVictoryTween.Parallel().TweenProperty(scoreText, (string)CanvasItem.PropertyName.Modulate + ":a", ALPHA_OPAQUE, scoreMinDuration);

        lVictoryTween.Parallel().TweenProperty(scoreLabel, (string)CanvasItem.PropertyName.Modulate + ":a", ALPHA_OPAQUE, scoreMinDuration * HALF_MULTIPLIER);

        float lDynamicDuration = CalculateScoreDuration(GameStats.currentScore);

        lVictoryTween.Parallel().TweenMethod(Callable.From<int>(UpdateScoreVisuals), START_VALUE_ZERO, GameStats.currentScore, lDynamicDuration)
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.Out);

        lVictoryTween.Chain().TweenProperty(scoreLabel, (string)CanvasItem.PropertyName.Modulate, Colors.White, scoreCoolDownDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);

        lVictoryTween.Parallel().TweenProperty(scoreLabel, (string)Control.PropertyName.Scale, baseScaleScore, scoreCoolDownDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        lVictoryTween.TweenInterval(stepDelay);

        int lRankIndex = GetRankIndex(GameStats.currentScore);
        string lRankText = GetRankTitle(lRankIndex);
        Color lRankColor = GetRankColor(lRankIndex);

        lVictoryTween.TweenCallback(Callable.From(() =>
        {
            rankLabel.Text = lRankText;
            rankLabel.Modulate = new Color(lRankColor.R, lRankColor.G, lRankColor.B, ALPHA_TRANSPARENT);
            rankLabel.PivotOffset = rankLabel.Size * PIVOT_CENTER_RATIO;
            rankLabel.Scale = baseScaleRank * RANK_SCALE_HUGE_MULT;
            rankLabel.RotationDegrees = RANK_ROTATION_START;
        }));

        lVictoryTween.Parallel().TweenProperty(rankLabel, (string)CanvasItem.PropertyName.Modulate + ":a", ALPHA_OPAQUE, ANIM_SLAM_FADE_DURATION);

        lVictoryTween.Parallel()
            .TweenProperty(rankLabel, (string)Control.PropertyName.RotationDegrees, RANK_ROTATION_SLAM, ANIM_SLAM_DURATION)
            .SetTrans(Tween.TransitionType.Quint)
            .SetEase(Tween.EaseType.In);

        lVictoryTween.Parallel()
            .TweenProperty(rankLabel, (string)Control.PropertyName.Scale, baseScaleRank * RANK_SCALE_SLAM_MULT, ANIM_SLAM_DURATION)
            .SetTrans(Tween.TransitionType.Quint)
            .SetEase(Tween.EaseType.In);

        lVictoryTween.TweenCallback(Callable.From(ShakeRankLabel));

        lVictoryTween.TweenProperty(rankLabel, (string)Control.PropertyName.Scale, baseScaleRank, rankStampDuration)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);

        lVictoryTween.Finished += () => MenuManager.GetInstance().loading = false;
    }

    private void UpdateScoreVisuals(int pCurrentValue)
    {
        if (scoreLabel == null) return;

        scoreLabel.Text = pCurrentValue.ToString();
        scoreLabel.PivotOffset = scoreLabel.Size * PIVOT_CENTER_RATIO;

        float lTargetScore = Math.Max(GameStats.currentScore, DEFAULT_SCORE_MIN);
        float lRatio = pCurrentValue / lTargetScore;

        scoreLabel.Modulate = Colors.White.Lerp(scoreHeatColor, lRatio);

        scoreLabel.Scale = baseScaleScore.Lerp(baseScaleScore * scoreHeatScaleMultiplier, lRatio);
    }

    private float CalculateScoreDuration(int pScore)
    {
        float lRatio = pScore / (float)scoreForMaxDuration;
        lRatio = Mathf.Clamp(lRatio, CLAMP_MIN, CLAMP_MAX);

        return scoreMinDuration + (scoreMaxDuration - scoreMinDuration) * lRatio;
    }

    private void ShakeRankLabel()
    {
        if (rankLabel == null) return;

        Tween lShakeTween = CreateTween();
        Vector2 lOriginalPos = rankLabel.Position;

        Vector2 lRightDown = new Vector2(shakeStrength, shakeStrength);
        Vector2 lLeft = new Vector2(-shakeStrength, 0);
        Vector2 lUp = new Vector2(0, -shakeStrength);

        lShakeTween.TweenProperty(rankLabel, (string)Control.PropertyName.Position, lOriginalPos + lRightDown, ANIM_SHAKE_STEP_DURATION);
        lShakeTween.TweenProperty(rankLabel, (string)Control.PropertyName.Position, lOriginalPos + lLeft, ANIM_SHAKE_STEP_DURATION);
        lShakeTween.TweenProperty(rankLabel, (string)Control.PropertyName.Position, lOriginalPos + lUp, ANIM_SHAKE_STEP_DURATION);
        lShakeTween.TweenProperty(rankLabel, (string)Control.PropertyName.Position, lOriginalPos, ANIM_SHAKE_STEP_DURATION);
    }

    private void AddStatToSequence(Tween pSequenceTween, Label pTextLabel, Label pValueLabel, int pTargetValue, Vector2 pBaseScaleValue)
    {
        if (pTextLabel != null)
        {
            pSequenceTween.Parallel().TweenProperty(pTextLabel, (string)CanvasItem.PropertyName.Modulate + ":a", ALPHA_OPAQUE, statFadeDuration)
               .SetEase(Tween.EaseType.Out);
        }

        if (pValueLabel != null)
        {
            pValueLabel.Modulate = Colors.Transparent;
            pValueLabel.Scale = pBaseScaleValue * SCALE_HIDDEN_MULT;

            Color lFlashColor = new Color(COLOR_OVERBRIGHT_VAL, COLOR_OVERBRIGHT_VAL, COLOR_OVERBRIGHT_VAL, ALPHA_OPAQUE);

            pSequenceTween.Parallel().TweenProperty(pValueLabel, (string)CanvasItem.PropertyName.Modulate + ":a", ALPHA_OPAQUE, statFadeDuration)
                .SetEase(Tween.EaseType.Out);

            pSequenceTween.Parallel().TweenMethod(Callable.From<int>(pValue =>
            {
                pValueLabel.Text = pValue.ToString();
                pValueLabel.PivotOffset = pValueLabel.Size * PIVOT_CENTER_RATIO;
            }), START_VALUE_ZERO, pTargetValue, statCountDuration
            ).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);

            pSequenceTween.Parallel()
                .TweenProperty(pValueLabel, (string)Control.PropertyName.Scale, pBaseScaleValue * statPopMultiplier, statCountDuration * STAT_POP_UP_RATIO)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);

            pSequenceTween.Parallel()
                .TweenProperty(pValueLabel, (string)Control.PropertyName.Scale, pBaseScaleValue, statCountDuration * STAT_POP_DOWN_RATIO)
                .SetDelay(statCountDuration * STAT_POP_UP_RATIO)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Elastic);

            pSequenceTween.Parallel()
                .TweenProperty(pValueLabel, (string)CanvasItem.PropertyName.Modulate, lFlashColor, STAT_FLASH_IN_DURATION)
                .SetEase(Tween.EaseType.Out);

            pSequenceTween.Parallel()
                .TweenProperty(pValueLabel, (string)CanvasItem.PropertyName.Modulate, Colors.White, STAT_FLASH_OUT_DURATION)
                .SetDelay(STAT_FLASH_IN_DURATION);
        }

        pSequenceTween.TweenInterval(stepDelay);
    }

    private int GetRankIndex(int pScore)
    {
        int lResultIndex = 0;

        if (rankScoreThresholds == null || rankScoreThresholds.Length == 0)
            return lResultIndex;

        for (int i = 0; i < rankScoreThresholds.Length; i++)
        {
            if (pScore >= rankScoreThresholds[i])
                lResultIndex = i;
        }

        return lResultIndex;
    }

    private string GetRankTitle(int pIndex)
    {
        if (rankTitles == null || pIndex < 0 || pIndex >= rankTitles.Length)
            return DEFAULT_RANK_TITLE;

        return rankTitles[pIndex];
    }

    private Color GetRankColor(int pIndex)
    {
        if (rankColors == null || pIndex < 0 || pIndex >= rankColors.Length)
            return Colors.White;

        return rankColors[pIndex];
    }
}