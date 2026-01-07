using Godot;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;

public partial class Hammer : GameObject
{
    // ========================================================================
    // ENUMS & CONSTANTS
    // ========================================================================

    public enum State { Idle, Charging, Swing, Cooldown }

    // Valeurs techniques fixes
    private const float BASE_PITCH = 1.0f;
    private const float MIN_RATIO = 0.0f;
    private const float MAX_RATIO = 1.0f;

    // ========================================================================
    // VARIABLES
    // ========================================================================

    public State currentState;
    public bool isActif;

    [ExportGroup("References")]
    [Export] private TextureProgressBar progressBar;
    [Export] private CollisionShape2D collision;
    private Player player;

    [ExportGroup("Configuration")]
    [Export] private HammerPhase[] phases;
    [Export] private float minChargeDuration = 0.5f;
    [Export] private float idleRotation = 120f;
    [Export] private float hammerHideDelay = 0.2f;

    [ExportGroup("Visuals - Colors")]
    [Export] private Color colorDeadZone = Colors.LightSlateGray;
    [Export] private Color colorAttack = Colors.Red;
    [Export] private Color colorCooldown = Colors.Cyan;

    [ExportGroup("Visuals - Juice & Feel")]
    [Export(PropertyHint.Range, "0, 2, 0.1")] private float scaleElasticity = 0.2f;      // Durée du rebond élastique
    [Export(PropertyHint.Range, "1, 2, 0.1")] private float chargeStartScaleMult = 1.2f; // Grossissement au début de la charge (Pop)
    [Export(PropertyHint.Range, "1, 2, 0.1")] private float attackImpactScaleMult = 1.3f; // Grossissement à l'impact (Hit)
    [Export(PropertyHint.Range, "0, 1, 0.05")] private float attackImpactDuration = 0.1f; // Vitesse du pop d'impact
    [Export(PropertyHint.Range, "0, 1, 0.05")] private float returnToIdleDuration = 0.2f; // Vitesse retour taille normale

    [ExportGroup("Visuals - HUD Bump")]
    [Export] private float hudBumpChargeMult = 1.3f; // Bump de la barre quand on change de phase
    [Export] private float hudBumpAttackMult = 1.1f; // Bump de la barre quand on attaque

    [ExportGroup("Attack")]
    public int attackForce;

    [ExportGroup("Audio")]
    [Export] private AudioManager.AudioType chargeSoundType = AudioManager.AudioType.HammerCharge;
    [Export] private AudioManager.AudioType swingSoundType = AudioManager.AudioType.HammerSwing;
    [Export] private float maxChargePitch = 3.0f;

    // --- Technical ---
    private Tween animationTween;
    private Tween scaleTween;
    private Timer hideTimer;
    private AudioStreamPlayer sfxPlayer;

    private float totalChargeTime;
    private float totalMaxTimeCache;
    private float cooldownDuration;
    private float cooldownTimer;

    private int currentPhaseIndex = -1;
    private HammerPhase currentPhaseData;

    private static Hammer instance;

    private readonly string propRotation = (string)Node2D.PropertyName.RotationDegrees;
    private readonly string propScale = (string)Node2D.PropertyName.Scale;

    public static Hammer GetInstance() => instance;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else { QueueFree(); return; }

        sfxPlayer = new AudioStreamPlayer();
        sfxPlayer.Bus = "SFX";
        AddChild(sfxPlayer);

        hideTimer = new Timer();
        hideTimer.OneShot = true;
        hideTimer.WaitTime = hammerHideDelay;
        hideTimer.Timeout += FinishAttackAnimation;
        AddChild(hideTimer);

        if (phases == null || phases.Length == 0)
        {
            GD.PrintErr("HAMMER: Aucune phase assignée !");
            SetProcess(false);
        }
    }

    public override void Initiate()
    {
        base.Initiate();
        enable = false;

        baseScale = sprite.Scale;
        if (phases != null && phases.Length > 0)
        {
            sprite.Scale = baseScale * phases[0].Scale;
        }

        currentState = State.Idle;
        totalChargeTime = 0;
        cooldownTimer = 0;
        attackForce = 0;
        currentPhaseIndex = -1;

        animationTween?.Kill();
        animationTween = null;

        SetRotation(idleRotation, 0);
    }

    public void SetPlayer(Player pPlayer) => player = pPlayer;

    // ========================================================================
    // STATE MACHINE
    // ========================================================================

    public void Attack(float pDelta)
    {
        if (animationTween != null && animationTween.IsValid())
            animationTween.CustomStep(pDelta);

        switch (currentState)
        {
            case State.Idle:
                if (Input.IsActionJustPressed(Property.INPUT_SPECIAL))
                    StartCharging();
                break;

            case State.Charging:
                UpdateChargingLogic(pDelta);

                if (Input.IsActionJustReleased(Property.INPUT_SPECIAL))
                    TryReleaseAttack();
                break;

            case State.Swing:
                break;

            case State.Cooldown:
                UpdateCooldownLogic(pDelta);
                break;
        }
    }

    // ========================================================================
    // CHARGING LOGIC
    // ========================================================================

    private void StartCharging()
    {
        isActif = true;
        progressBar.Visible = true;
        sprite.Visible = true;
        currentState = State.Charging;

        totalChargeTime = 0;
        currentPhaseIndex = -1;
        currentPhaseData = null;
        attackForce = 0;

        totalMaxTimeCache = GetTotalMaxChargeTime();

        progressBar.MaxValue = totalMaxTimeCache;
        progressBar.Value = 0;
        progressBar.TintProgress = colorDeadZone;

        Effect.SquashStretch(player.sprite, player.baseSpriteScale, false);

        PlaySoundWithPitch(chargeSoundType, BASE_PITCH);

        if (phases.Length > 0)
        {
            float lFirstTarget = phases[0].TargetChargeRotation;
            float lDuration = minChargeDuration + phases[0].Duration;
            SetRotation(lFirstTarget, lDuration);

            // Pop initial de charge (Variable Exportée)
            SetScale(phases[0].Scale * chargeStartScaleMult, scaleElasticity);
        }
    }

    private void UpdateChargingLogic(float pDelta)
    {
        totalChargeTime += pDelta;
        progressBar.Value = totalChargeTime;

        // === AUDIO PITCH DYNAMIQUE ===
        float lGlobalRatio = Mathf.Clamp(totalChargeTime / totalMaxTimeCache, MIN_RATIO, MAX_RATIO);

        // Acceleration quadratique (EaseIn)
        float lPitchMultiplier = lGlobalRatio * lGlobalRatio;
        sfxPlayer.PitchScale = Mathf.Lerp(BASE_PITCH, maxChargePitch, lPitchMultiplier);
        // =============================

        if (totalChargeTime < minChargeDuration) return;

        float lActiveTime = totalChargeTime - minChargeDuration;
        float lAccumulatedDuration = 0f;
        int lNewIndex = -1;
        float lProgressInPhase = 0f;

        for (int i = 0; i < phases.Length; i++)
        {
            if (lActiveTime < lAccumulatedDuration + phases[i].Duration)
            {
                lNewIndex = i;
                lProgressInPhase = (lActiveTime - lAccumulatedDuration) / phases[i].Duration;
                break;
            }
            lAccumulatedDuration += phases[i].Duration;
        }

        if (lNewIndex == -1)
        {
            ApplyPhase(phases.Length - 1);
            StartSwing();
            return;
        }

        if (lNewIndex != currentPhaseIndex)
        {
            ApplyPhase(lNewIndex);

            Effect.BumpHUD(progressBar, progressBar.Scale, hudBumpChargeMult);

            SetRotation(currentPhaseData.TargetChargeRotation, currentPhaseData.Duration);
            SetScale(currentPhaseData.Scale, scaleElasticity);
        }

        UpdateColorsOnly(lProgressInPhase);
    }

    private void ApplyPhase(int pIndex)
    {
        currentPhaseIndex = pIndex;
        currentPhaseData = phases[currentPhaseIndex];
        attackForce = currentPhaseData.Force;
    }

    private void UpdateColorsOnly(float pProgress)
    {
        Color lStartColor;
        if (currentPhaseIndex == 0) lStartColor = colorDeadZone;
        else lStartColor = phases[currentPhaseIndex - 1].BarColor;

        progressBar.TintProgress = lStartColor.Lerp(currentPhaseData.BarColor, pProgress);
    }

    private void TryReleaseAttack()
    {
        if (totalChargeTime < minChargeDuration)
        {
            CancelCharge();
            return;
        }
        StartSwing();
    }

    private void CancelCharge() => ResetToIdle();

    // ========================================================================
    // ATTACK & COOLDOWN
    // ========================================================================

    private void StartSwing()
    {
        if (currentPhaseData == null) { CancelCharge(); return; }

        currentState = State.Swing;
        cooldownDuration = totalChargeTime;
        cooldownTimer = cooldownDuration;

        collision.Disabled = false;
        progressBar.TintProgress = colorAttack;

        player.Dash(currentPhaseData.DashForce, Vector2.Right, false);
        player.invincible = true;

        // Bump HUD Attack (Variable Exportée)
        Effect.BumpHUD(progressBar, progressBar.Scale, hudBumpAttackMult);

        PlayHammerSoundForDuration(swingSoundType, currentPhaseData.SwingDuration);

        animationTween = SetRotation(currentPhaseData.TargetSwingRotation, currentPhaseData.SwingDuration);
        animationTween.Finished += FinishAttack;

        // Scale Impact Attack (Variables Exportées)
        SetScale(currentPhaseData.Scale * attackImpactScaleMult, attackImpactDuration);
    }

    private void FinishAttack()
    {
        isActif = false;
        collision.Disabled = true;

        float lBaseScaleMultiplier = (phases != null && phases.Length > 0) ? phases[0].Scale : 1.0f;

        SetScale(lBaseScaleMultiplier, returnToIdleDuration);

        hideTimer.Start();
        currentState = State.Cooldown;
        progressBar.TintProgress = colorCooldown;
        progressBar.Value = cooldownDuration;
    }

    private void UpdateCooldownLogic(float pDelta)
    {
        cooldownTimer -= pDelta;
        progressBar.Value = cooldownTimer;

        isActif = false;

        if (cooldownTimer <= 0)
            ResetToIdle();
    }

    private void FinishAttackAnimation()
    {
        sprite.Visible = false;
        player.invincible = false;
    }

    public void ResetToIdle()
    {
        if (sfxPlayer.Playing) sfxPlayer.Stop();

        float lBaseScaleMultiplier = (phases != null && phases.Length > 0) ? phases[0].Scale : 1.0f;

        SetScale(lBaseScaleMultiplier, returnToIdleDuration);

        totalChargeTime = 0;
        currentPhaseIndex = -1;
        progressBar.Visible = false;
        sprite.Visible = false;
        isActif = false;
        attackForce = 0;

        currentState = State.Idle;
        SetRotation(idleRotation, 0);
    }

    // ========================================================================
    // TOOLS
    // ========================================================================

    private float GetTotalMaxChargeTime()
    {
        float lTotal = minChargeDuration;
        if (phases != null)
        {
            foreach (var phase in phases) lTotal += phase.Duration;
        }
        return lTotal;
    }

    private Tween SetRotation(float pTarget, float pTime)
    {
        animationTween?.Kill();

        if (pTime <= 0)
        {
            sprite.RotationDegrees = pTarget;
            animationTween = null;
            return null;
        }

        animationTween = CreateTween();
        animationTween.TweenProperty(sprite, propRotation, pTarget, pTime)
              .SetEase(Tween.EaseType.Out)
              .SetTrans(Tween.TransitionType.Back);

        animationTween.Pause();
        return animationTween;
    }

    private void SetScale(float pTargetScaleMultiplier, float pTime)
    {
        scaleTween?.Kill();
        scaleTween = CreateTween();

        Vector2 lTargetScale = baseScale * pTargetScaleMultiplier;

        if (pTime <= 0)
        {
            sprite.Scale = lTargetScale;
            return;
        }

        scaleTween.TweenProperty(sprite, propScale, lTargetScale, pTime)
              .SetEase(Tween.EaseType.Out)
              .SetTrans(Tween.TransitionType.Back);
    }

    private void PlaySoundWithPitch(AudioManager.AudioType pType, float pPitch)
    {
        AudioStream lStream = AudioManager.GetInstance().GetStream(pType);
        if (lStream == null) return;

        if (sfxPlayer.Stream != lStream || !sfxPlayer.Playing)
        {
            sfxPlayer.Stream = lStream;
            sfxPlayer.Play();
        }
        sfxPlayer.PitchScale = pPitch;
    }

    private void PlayHammerSoundForDuration(AudioManager.AudioType pType, float pTargetDuration)
    {
        AudioStream lStream = AudioManager.GetInstance().GetStream(pType);
        if (lStream == null) return;

        sfxPlayer.Stop();
        sfxPlayer.Stream = lStream;

        float lOriginalDuration = (float)lStream.GetLength();
        if (lOriginalDuration > 0 && pTargetDuration > 0)
            sfxPlayer.PitchScale = lOriginalDuration / pTargetDuration;
        else
            sfxPlayer.PitchScale = BASE_PITCH;

        sfxPlayer.Play();
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}