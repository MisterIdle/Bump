using Godot;
using System;

public partial class Hammer : GameObject
{
    private enum State { 
        Idle, 
        Charging, 
        Charged, 
        Swing, 
        Cooldown 
    }

    private State state;

    public bool isActif;

    [ExportGroup("References")]
    [Export] private Sprite2D sprite;
    [Export] private TextureProgressBar progressBar;
    [Export] private CollisionShape2D collision;

    [ExportGroup("Charge")]
    [Export] private float maxChargeTime = 1f;
    [Export] private float maxChargedTime = 3f;

    [ExportGroup("Attack")]
    [Export] private float baseAttackTime = 0.1f;
    [Export] private float attackTimeGrowth = 0.15f;
    [Export] private float attackTimeDouble = 1.5f;

    [ExportGroup("Rotation")]
    [Export] private float idleRotation = 120f;
    [Export] private float startChargeRotation = -20f;
    [Export] private float chargedRotation = -60f;
    [Export] private float swingRotation = 120f;

    [ExportGroup("Tween")]
    [Export] private float chargeTweenTime = 0.1f;
    [Export] private float chargedTweenTime = 3f;

    [ExportGroup("Misc")]
    [Export] private float hammerHide = 0.5f;

    private Tween animationTween;
    private Timer animationTimer;

    private float chargeTime;
    private float attackTime;

    private static Hammer instance;

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
    }

    public override void Initiate()
    {
        base.Initiate();
        enable = false;

        state = State.Idle;
        attackTime = baseAttackTime;

        animationTween = CreateTween();

        animationTimer = new Timer();
        animationTimer.WaitTime = hammerHide;
        animationTimer.OneShot = true;
        animationTimer.Timeout += FinishAttackAnimation;

        AddChild(animationTimer);

        SetRotation(idleRotation, maxChargeTime);
    }

    public void Attack(float pDelta)
    {
        progressBar.Value = chargeTime;

        if (chargeTime <= 0) chargeTime = 0;

        switch (state)
        {
            case State.Idle:
                if (Input.IsActionJustPressed(Property.INPUT_SPECIAL))
                    StartCharging();
                break;

            case State.Charging:
                chargeTime += pDelta;
                progressBar.TintProgress = LerpColors(Colors.Red, Colors.Orange, chargeTime);


                if (chargeTime >= maxChargeTime)
                    FullyCharged();

                if (Input.IsActionJustReleased(Property.INPUT_SPECIAL))
                    ResetToIdle();

                break;

            case State.Charged:
                chargeTime += pDelta;
                attackTime += attackTimeGrowth * pDelta;

                progressBar.TintProgress = LerpColors(Colors.Orange, Colors.Green, chargeTime / maxChargedTime);

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

        state = State.Charging;
        SetRotation(startChargeRotation, maxChargeTime - chargeTweenTime);
    }

    private void FullyCharged()
    {
        state = State.Charged;
        Effect.BumpHUD(progressBar, 1.1f);
        SetRotation(chargedRotation, chargedTweenTime);
    }

    private void StartSwing()
    {
        state = State.Swing;

        collision.Disabled = false;

        progressBar.TintProgress = Colors.Red;

        Player.GetInstance().Dash(attackTime / 2, Vector2.Right, false);
        Player.GetInstance().invincible = true;

        Effect.BumpHUD(progressBar, 1.1f);
        animationTween?.Kill();

        animationTween = SetRotation(swingRotation, attackTime);
        animationTween.Finished += FinishAttack;
    }

    private void FinishAttack()
    {
        isActif = false;
        collision.Disabled = true;
        state = State.Cooldown;
        animationTimer.Start();
    }

    private void FinishAttackAnimation()
    {
        sprite.Visible = false;
        Player.GetInstance().invincible = false;
    }

    private void EndCooldown()
    {
        attackTime = baseAttackTime;
        ResetToIdle();
    }

    public void ResetToIdle()
    {
        chargeTime = 0;

        progressBar.Visible = false;
        sprite.Visible = false;

        state = State.Idle;
        SetRotation(idleRotation, 0);
    }


    private Tween SetRotation(float pTarget, float pTime)
    {
        animationTween?.Kill();

        animationTween = CreateTween();
        animationTween.TweenProperty(sprite, (string)PropertyName.RotationDegrees, pTarget, pTime)
             .SetEase(Tween.EaseType.InOut)
             .SetTrans(Tween.TransitionType.Back);

        return animationTween;
    }

    private Color LerpColors(Color pColorA, Color ColorB, float pTime)
    {
        return pColorA.Lerp(ColorB, Mathf.Clamp(pTime, 0f, 1f));
    }
    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}