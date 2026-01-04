using Godot;
using System;

public partial class Hammer : GameObject
{
    public enum State
    {
        Idle,
        Charging,
        Charged,
        Swing,
        Cooldown
    }

    public State currentState;
    public bool isActif;

    [ExportGroup("References")]
    [Export] private TextureProgressBar progressBar;
    [Export] private CollisionShape2D collision;
    private Player player;

    [ExportGroup("Charge")]
    [Export] private float maxChargeTime = 1f;
    [Export] private float maxChargedTime = 3f;

    [ExportGroup("Attack")]
    public int attackForce;
    private int attackForceMultiply = 3;
    [Export] private float baseAttackTime = 0.1f;
    [Export] private float attackTimeGrowth = 0.15f;

    [ExportGroup("Rotation")]
    [Export] private float idleRotation = 120f;
    [Export] private float startChargeRotation = -20f;
    [Export] private float chargedRotation = -60f;
    [Export] private float[] swingRotations;

    [ExportGroup("Scale")]
    [Export] private float scaleCharged = 1.2f;
    [Export] private float scaleTime = 0.2f;

    [ExportGroup("Tween")]
    [Export] private float chargeTweenTime = 0.1f;
    [Export] private float chargedTweenTime = 3f;

    [ExportGroup("Misc")]
    [Export] private float hammerHide = 0.2f;
    [Export] private float dashDistance = 0.15f;

    [ExportGroup("Audio")]
    [Export] private AudioManager.AudioType chargeSoundType = AudioManager.AudioType.HammerCharge;
    [Export] private AudioManager.AudioType swingSoundType = AudioManager.AudioType.HammerSwing;

    private Tween animationTween;
    private Timer animationTimer;
    private AudioStreamPlayer sfxPlayer;

    private float chargeTime;
    private float attackTime;

    private static Hammer instance;

    private readonly string propRotation = (string)Node2D.PropertyName.RotationDegrees;
    private readonly string propScale = (string)Node2D.PropertyName.Scale;

    public static Hammer GetInstance()
    {
        return instance;
    }

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else
        {
            QueueFree();
            return;
        }

        sfxPlayer = new AudioStreamPlayer();
        sfxPlayer.Bus = "SFX";
        AddChild(sfxPlayer);
    }

    public override void Initiate()
    {
        base.Initiate();
        enable = false;

        baseScale = sprite.Scale;
        currentState = State.Idle;
        attackTime = baseAttackTime;

        animationTween = CreateTween();
        animationTween.Kill();

        animationTimer = new Timer();
        animationTimer.WaitTime = hammerHide;
        animationTimer.OneShot = true;
        animationTimer.Timeout += FinishAttackAnimation;

        AddChild(animationTimer);

        SetRotation(idleRotation, maxChargeTime);
    }

    public void SetPlayer(Player pPlayer)
    {
        player = pPlayer;
    }

    public void Attack(float pDelta)
    {
        if (animationTween != null && animationTween.IsValid())
            animationTween.CustomStep(pDelta);

        progressBar.Value = chargeTime;

        if (chargeTime <= 0)
            chargeTime = 0;

        switch (currentState)
        {
            case State.Idle:
                if (Input.IsActionJustPressed(Property.INPUT_SPECIAL))
                    StartCharging();
                break;

            case State.Charging:
                chargeTime += pDelta;
                progressBar.TintProgress = LerpColors(Colors.OrangeRed, Colors.Orange, chargeTime);

                if (chargeTime >= maxChargeTime)
                    FullyCharged();

                if (Input.IsActionJustReleased(Property.INPUT_SPECIAL))
                    ResetToIdle();
                break;

            case State.Charged:
                chargeTime += pDelta;
                attackTime += attackTimeGrowth * pDelta;
                attackForce = (int)chargeTime;

                switch (attackForce)
                {
                    case 1:
                        progressBar.TintProgress = LerpColors(Colors.Orange, Colors.GreenYellow, chargeTime / maxChargedTime);
                        break;
                    case 2:
                        progressBar.TintProgress = LerpColors(Colors.GreenYellow, Colors.Green, chargeTime / maxChargedTime);
                        break;
                }

                if (Input.IsActionJustReleased(Property.INPUT_SPECIAL) || chargeTime > maxChargedTime)
                    StartSwing();
                break;

            case State.Cooldown:
                chargeTime -= pDelta;
                if (chargeTime <= 0f)
                    EndCooldown();
                break;
        }
    }

    private void StartCharging()
    {
        isActif = true;
        progressBar.Visible = true;
        sprite.Visible = true;
        currentState = State.Charging;

        Effect.SquashStretch(player.sprite, player.baseSpriteScale, false);
        SetRotation(startChargeRotation, maxChargeTime - chargeTweenTime);
        Effect.BumpScale(sprite, baseSpriteScale);

        PlayHammerSound(chargeSoundType, maxChargedTime);
    }

    private void FullyCharged()
    {
        currentState = State.Charged;
        Effect.BumpHUD(progressBar, progressBar.Scale, 1.1f);
        SetRotation(chargedRotation, chargedTweenTime);
        SetScale(scaleCharged, scaleTime);
    }

    private void StartSwing()
    {
        currentState = State.Swing;
        Effect.SquashStretch(player.sprite, player.baseSpriteScale, false);
        collision.Disabled = false;
        progressBar.TintProgress = Colors.OrangeRed;

        player.Dash(dashDistance * attackForce, Vector2.Right, false);
        player.invincible = true;

        Effect.BumpHUD(progressBar, progressBar.Scale, 1.1f);

        animationTween?.Kill();

        int lAttackForceIndex = attackForce - 1;
        float lTotalDuration = attackTime * attackForce;

        PlayHammerSound(swingSoundType, lTotalDuration);

        if (lAttackForceIndex < swingRotations.Length)
            animationTween = SetRotation(swingRotations[lAttackForceIndex], lTotalDuration);

        attackForce = attackForceMultiply * (int)chargeTime;

        animationTween.Finished += FinishAttack;
    }

    private void PlayHammerSound(AudioManager.AudioType pType, float pTargetDuration)
    {
        AudioStream lStream = AudioManager.GetInstance().GetStream(pType);

        if (lStream == null || pTargetDuration <= 0) return;

        sfxPlayer.Stop();
        sfxPlayer.Stream = lStream;

        float lOriginalDuration = (float)lStream.GetLength();
        if (lOriginalDuration > 0)
        {
            sfxPlayer.PitchScale = lOriginalDuration / pTargetDuration;
        }

        sfxPlayer.Play();
    }

    private void FinishAttack()
    {
        isActif = false;
        collision.Disabled = true;
        Effect.BumpScale(sprite, baseSpriteScale);
        currentState = State.Cooldown;
        animationTimer.Start();
    }

    private void FinishAttackAnimation()
    {
        sprite.Visible = false;
        player.invincible = false;
    }

    private void EndCooldown()
    {
        attackTime = baseAttackTime;
        ResetToIdle();
    }

    public void ResetToIdle()
    {
        if (sfxPlayer.Playing)
            sfxPlayer.Stop();

        SetScale(1, 0);
        chargeTime = 0;
        progressBar.Visible = false;
        sprite.Visible = false;
        isActif = false;
        currentState = State.Idle;
        SetRotation(idleRotation, 0);
    }

    private Tween SetRotation(float pTarget, float pTime)
    {
        animationTween?.Kill();

        animationTween = CreateTween();
        animationTween.TweenProperty(sprite, propRotation, pTarget, pTime)
              .SetEase(Tween.EaseType.InOut)
              .SetTrans(Tween.TransitionType.Back);

        animationTween.Pause();

        return animationTween;
    }

    private void SetScale(float pScale, float pTime)
    {
        Tween lTween = CreateTween();
        lTween.TweenProperty(sprite, propScale, baseScale * pScale, pTime)
              .SetEase(Tween.EaseType.InOut)
              .SetTrans(Tween.TransitionType.Back);
    }

    private Color LerpColors(Color pColorA, Color pColorB, float pTime)
    {
        return pColorA.Lerp(pColorB, Mathf.Clamp(pTime, 0f, 1f));
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}