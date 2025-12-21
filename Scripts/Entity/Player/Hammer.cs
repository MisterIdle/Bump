using Godot;

public partial class Hammer : GameObject
{
    private enum State { Idle, Charging, Charged, Swing, Cooldown }
    private State state;

    [ExportGroup("References")]
    [Export] private Sprite2D sprite;
    [Export] private TextureProgressBar progressBar;
    [Export] private CollisionShape2D collision;
    //[Export] private GpuParticles2D particule;

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
    [Export] private float hammerHide = 0.2f;

    private Tween tween;
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
        else QueueFree();
    }

    public override void Initiate()
    {
        base.Initiate();
        enable = false;

        state = State.Idle;
        attackTime = baseAttackTime;

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
                if (Input.IsActionJustPressed("SPECIAL"))
                    StartCharging();
                break;

            case State.Charging:
                chargeTime += pDelta;
                progressBar.TintProgress = LerpColors(Colors.Red, Colors.Orange, chargeTime);


                if (chargeTime >= maxChargeTime)
                    FullyCharged();

                if (Input.IsActionJustReleased("SPECIAL"))
                    ResetToIdle();

                break;

            case State.Charged:
                chargeTime += pDelta;
                attackTime += attackTimeGrowth * pDelta;

                progressBar.TintProgress = LerpColors(Colors.Orange, Colors.Green, chargeTime / maxChargedTime);

                if (Input.IsActionJustReleased("SPECIAL") || chargeTime > maxChargedTime)
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
        //particule.Emitting = true;
        progressBar.Visible = true;
        sprite.Visible = true;

        state = State.Charging;
        SetRotation(startChargeRotation, maxChargeTime - chargeTweenTime);
    }

    private void FullyCharged()
    {
        state = State.Charged;
        SetRotation(chargedRotation, chargedTweenTime);
    }

    private void StartSwing()
    {
        state = State.Swing;

        collision.Disabled = false;

        progressBar.TintProgress = Colors.Red;

        Player.GetInstance().invulnerability = true;
        Player.GetInstance().Dash(attackTime / 2);

        if (tween != null && tween.IsRunning()) tween.Kill();

        tween = SetRotation(swingRotation, attackTime);
        tween.Finished += FinishAttack;
    }


    private void EndCooldown()
    {
        attackTime = baseAttackTime;
        ResetToIdle();
    }

    private void ResetToIdle()
    {
        chargeTime = 0;

        progressBar.Visible = false;
        //particule.Emitting = true;
        sprite.Visible = false;

        state = State.Idle;
        SetRotation(idleRotation, 0);
    }

    private void FinishAttack()
    {
        collision.Disabled = true;
        state = State.Cooldown;
        animationTimer.Start();
        Player.GetInstance().invulnerability = false;
    }
    private void FinishAttackAnimation()
    {
        sprite.Visible = false;
        //particule.Emitting = true;
    }

    private Tween SetRotation(float pTarget, float pTime)
    {
        if (tween != null && tween.IsRunning()) tween.Kill();

        tween = CreateTween();
        tween.TweenProperty(sprite, (string)PropertyName.RotationDegrees, pTarget, pTime)
             .SetEase(Tween.EaseType.InOut)
             .SetTrans(Tween.TransitionType.Back);

        return tween;
    }

    private Color LerpColors(Color pColorA, Color ColorB, float pTime)
    {
        return pColorA.Lerp(ColorB, Mathf.Clamp(pTime, 0f, 1f));
    }
}