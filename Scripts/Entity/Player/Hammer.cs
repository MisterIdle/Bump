using Godot;

using Com.IsartDigital.Shmup.NData;
using Com.IsartDigital.Shmup.NHUD;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NStats;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;

public partial class Hammer : GameObject
{
    public bool isActif;

    [ExportGroup("References")]
    [Export] private TextureProgressBar progressBar;
    [Export] private CollisionShape2D collision;
    private Player player;

    [ExportGroup("Configuration")]
    [Export] private HammerData[] phases;
    [Export] private float minChargeDuration = 0.5f;
    [Export] private float idleRotation = 120f;
    [Export] private float hammerHideDelay = 0.2f;
    [Export] private float cooldownExpFactor = 0.3f;

    [ExportGroup("Colors")]
    [Export] private Color colorDeadZone = Colors.LightSlateGray;
    [Export] private Color colorAttack = Colors.Red;
    [Export] private Color colorCooldown = Colors.DarkBlue;

    [ExportGroup("Juice")]
    [Export] private float scaleElasticity = 0.2f;
    [Export] private float attackImpactScaleMult = 1.3f;
    [Export] private float attackImpactDuration = 0.1f;
    [Export] private float returnToIdleDuration = 0.2f;

    [ExportGroup("Bump")]
    [Export] private float hudBumpChargeMult = 1.3f;
    [Export] private float hudBumpAttackMult = 1.1f;

    [ExportGroup("Attack")]
    public int attackForce;

    [ExportGroup("Audio")]
    [Export] private float maxChargePitch = 3.0f;

    private Tween animationTween;
    private Tween scaleTween;
    private Timer hideTimer;
    private AudioStreamPlayer sfxPlayer;

    private float totalChargeTime;
    private float totalMaxTimeCache;
    private float cooldownDuration;
    private float cooldownTimer;

    private int currentPhaseIndex = -1;
    private HammerData currentPhaseData;

    private Vector2 initialProgressBarScale;
    private Vector2 initialHudScale;

    public enum State { Idle, Charging, Swing, Cooldown }
    public State currentState;

    private const string BUS_SFX = "SFX";
    private const float BASE_PITCH = 1.0f;
    private const float MIN_RATIO = 0.0f;
    private const float MAX_RATIO = 1.0f;
    private const float DEFAULT_SCALE_MULTIPLIER = 1.0f;

    private static Hammer instance;
    public static Hammer GetInstance()
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

        sfxPlayer = new AudioStreamPlayer();
        sfxPlayer.Bus = BUS_SFX;
        AddChild(sfxPlayer);

        initialProgressBarScale = progressBar.Scale;

        hideTimer = new Timer();

        hideTimer.OneShot = true;
        hideTimer.WaitTime = hammerHideDelay;
        hideTimer.Timeout += FinishAttackAnimation;

        AddChild(hideTimer);
    }

    public override void Initiate()
    {
        base.Initiate();
        enable = false;

        baseScale = sprite.Scale;

        if (phases.Length > 0)
            sprite.Scale = baseScale * phases[0].scale;

        currentState = State.Idle;
        totalChargeTime = 0;
        cooldownTimer = 0;
        attackForce = 0;
        currentPhaseIndex = -1;

        animationTween?.Kill();
        animationTween = null;

        SetRotation(idleRotation, 0);
    }

    public void SetPlayer(Player pPlayer)
    {
        player = pPlayer;
    }

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

        totalMaxTimeCache = GetTotalMaxChargeTime();

        progressBar.MaxValue = totalMaxTimeCache;
        progressBar.Value = 0;
        progressBar.TintProgress = colorDeadZone;

        Effect.SquashStretch(player.sprite, player.baseSpriteScale, false);

        PlaySoundWithPitch(AudioManager.AudioType.HammerCharge, BASE_PITCH);

        if (phases.Length > 0)
        {
            float lFirstTarget = phases[0].targetChargeRotation;
            float lDuration = minChargeDuration + phases[0].duration;
            SetRotation(lFirstTarget, lDuration);
        }
    }

    private void UpdateChargingLogic(float pDelta)
    {
        float lActiveTime = totalChargeTime - minChargeDuration;
        float lAccumulatedDuration = 0f;
        int lNewIndex = -1;
        float lProgressInPhase = 0f;

        totalChargeTime += pDelta;
        progressBar.Value = totalChargeTime;

        float lGlobalRatio = Mathf.Clamp(totalChargeTime / totalMaxTimeCache, MIN_RATIO, MAX_RATIO);

        float lPitchMultiplier = lGlobalRatio * lGlobalRatio;
        sfxPlayer.PitchScale = Mathf.Lerp(BASE_PITCH, maxChargePitch, lPitchMultiplier);

        if (totalChargeTime < minChargeDuration) return;

        for (int i = 0; i < phases.Length; i++)
        {
            if (lActiveTime < lAccumulatedDuration + phases[i].duration)
            {
                lNewIndex = i;
                lProgressInPhase = (lActiveTime - lAccumulatedDuration) / phases[i].duration;
                break;
            }
            lAccumulatedDuration += phases[i].duration;
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

            Effect.BumpHUD(progressBar, initialProgressBarScale, hudBumpChargeMult);

            Effect.JoyVibration(attackImpactDuration, 0.5f, 1f);

            SetRotation(currentPhaseData.targetChargeRotation, currentPhaseData.duration);
            SetScale(currentPhaseData.scale, scaleElasticity);
        }

        UpdateColorsOnly(lProgressInPhase);
    }

    private void ApplyPhase(int pIndex)
    {
        currentPhaseIndex = pIndex;
        currentPhaseData = phases[currentPhaseIndex];
        attackForce = currentPhaseData.force;
    }

    private void UpdateColorsOnly(float pProgress)
    {
        Color lStartColor;

        if (currentPhaseIndex == 0)
            lStartColor = colorDeadZone;
        else
            lStartColor = phases[currentPhaseIndex - 1].barColor;

        progressBar.TintProgress = lStartColor.Lerp(currentPhaseData.barColor, pProgress);
    }

    private void TryReleaseAttack()
    {
        if (totalChargeTime < minChargeDuration)
        {
            ResetToIdle();
            return;
        }
        StartSwing();
    }

    // ========================================================================
    // ATTACK & COOLDOWN
    // ========================================================================

    private void StartSwing()
    {
        currentState = State.Swing;
        cooldownDuration = totalChargeTime;
        cooldownTimer = cooldownDuration;

        collision.Disabled = false;
        progressBar.TintProgress = colorAttack;

        player.Dash(currentPhaseData.dashForce, Vector2.Right, false);

        player.SetPositiveInvincibility(true);
        player.invincible = true;

        Effect.BumpHUD(progressBar, initialProgressBarScale, hudBumpAttackMult);
        Effect.BumpHUD(HUD.GetInstance(), HUD.GetInstance().Scale, hudBumpAttackMult);
        
        PlayRawSound(currentPhaseData.sound);

        animationTween = SetRotation(currentPhaseData.targetSwingRotation, currentPhaseData.swingDuration);
        animationTween.Finished += FinishAttack;

        GameStats.specialUsed++;

        SetScale(currentPhaseData.scale * attackImpactScaleMult, attackImpactDuration);

        Effect.JoyVibration(attackImpactDuration, 0.5f, 0.6f);
    }

    private void FinishAttack()
    {
        isActif = false;
        collision.Disabled = true;

        float lBaseScaleMultiplier = phases.Length > 0 ? phases[0].scale : DEFAULT_SCALE_MULTIPLIER;

        SetScale(lBaseScaleMultiplier, returnToIdleDuration);

        attackForce = 0;

        hideTimer.Start();
        currentState = State.Cooldown;
        progressBar.TintProgress = colorCooldown;
        progressBar.Value = cooldownDuration;
    }

    private void UpdateCooldownLogic(float pDelta)
    {
        float speed = Mathf.Exp(cooldownDuration * cooldownExpFactor);
        cooldownTimer -= pDelta * speed;

        progressBar.Value = cooldownTimer;
        isActif = false;

        if (cooldownTimer <= 0)
        {
            ResetToIdle();
            Effect.JoyVibration(0.2f);
        }
    }


    private void FinishAttackAnimation()
    {
        sprite.Visible = false;
        
        player.SetPositiveInvincibility(false);
        player.invincible = false;
    }

    public void ResetToIdle()
    {
        if (sfxPlayer.Playing)
            sfxPlayer.Stop();

        float lBaseScaleMultiplier = phases.Length > 0 ? phases[0].scale : DEFAULT_SCALE_MULTIPLIER;

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

        foreach (HammerData lPhase in phases)
            lTotal += lPhase.duration;

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
        animationTween.TweenProperty(sprite, (string)PropertyName.RotationDegrees, pTarget, pTime)
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

        scaleTween.TweenProperty(sprite, (string)PropertyName.Scale, lTargetScale, pTime)
              .SetEase(Tween.EaseType.Out)
              .SetTrans(Tween.TransitionType.Back);
    }

    private void PlaySoundWithPitch(AudioManager.AudioType pType, float pPitch)
    {
        AudioStream lStream = AudioManager.GetInstance().GetStream(pType);

        if (sfxPlayer.Stream != lStream || !sfxPlayer.Playing)
        {
            sfxPlayer.Stream = lStream;
            sfxPlayer.Play();
        }
        sfxPlayer.PitchScale = pPitch;
    }

    private void PlayRawSound(AudioStream pStream)
    {
        if (pStream == null) return;

        sfxPlayer.Stop();
        sfxPlayer.Stream = pStream;
        sfxPlayer.PitchScale = 1.0f;

        sfxPlayer.Play();
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}
