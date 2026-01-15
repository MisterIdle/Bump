using Godot;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class Rushroops : ShelledEnemy
{
    [ExportGroup("Animation")]
    [Export] private Node2D headVisuals;
    [Export] private float headOutDuration = 0.4f;
    [Export] private float headInDuration = 0.3f;
    [Export] private Vector2 shellPosOut;
    [Export] private Vector2 shellPosIn;

    [ExportGroup("State Timing")]
    [Export] private float spawnDuration = 2.0f;
    [Export] private float shootDuration = 2.0f;
    [Export] private float preDashDuration = 0.8f;

    [ExportGroup("Dash Settings")]
    [Export] private float dashSpeed = 800f;
    [Export] private float recoilDistance = 50f;
    [Export] private Vector2 escapeDirection = Vector2.Left;

    private enum EnemyState { Spawn, Shooting, PreDash, Dashing }
    private EnemyState currentState;
    private Timer stateTimer;

    private float defaultSpeed;
    private Tween animationTween;
    private Vector2 targetDashDirection;

    public override void _Ready()
    {
        base._Ready();

        stateTimer = new Timer();
        stateTimer.OneShot = true;
        stateTimer.Timeout += OnStateTimerTimeout;
        AddChild(stateTimer);
    }

    public override void Initiate()
    {
        base.Initiate();
        defaultSpeed = speed;
        baseScale = Scale;

        shell?.SetShellColor(Colors.Blue);
        shell.Position = shellPosOut;

        stateTimer.Stop();
        animationTween?.Kill();

        ChangeState(EnemyState.Spawn);
    }

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        switch (currentState)
        {
            case EnemyState.Spawn:
            case EnemyState.Shooting:
                velocity.X = angleX;
                break;

            case EnemyState.PreDash:
                velocity = new Vector2(angleX, angleY);
                break;

            case EnemyState.Dashing:
                break;
        }
    }

    private void ChangeState(EnemyState pNewState)
    {
        currentState = pNewState;

        switch (currentState)
        {
            case EnemyState.Spawn:
                speed = defaultSpeed;
                AnimateHead(false);
                stateTimer.Start(spawnDuration);
                break;

            case EnemyState.Shooting:
                speed = 0;
                affectedByScroll = true;
                wing?.SetSpeedDouble(false);

                AnimateHead(true);
                StartSpike();

                stateTimer.Start(shootDuration);
                break;

            case EnemyState.PreDash:
                StopSpike();
                shell?.SetShellColor(Colors.DarkBlue);
                wing?.SetSpeedDouble(true);

                targetDashDirection = Utils.DirectionTo(GlobalPosition, playerPos);

                PerformChargeAnimation();

                stateTimer.Start(preDashDuration);
                break;

            case EnemyState.Dashing:
                affectedByScroll = false;
                speed = dashSpeed;
                velocity = targetDashDirection;
                facePlayer = false;
                break;
        }
    }

    private void OnStateTimerTimeout()
    {
        if (isDeath) return;

        switch (currentState)
        {
            case EnemyState.Spawn:
                ChangeState(EnemyState.Shooting);
                break;

            case EnemyState.Shooting:
                ChangeState(EnemyState.PreDash);
                break;

            case EnemyState.PreDash:
                ChangeState(EnemyState.Dashing);
                break;
        }
    }

    private void AnimateHead(bool pIsOut)
    {
        animationTween?.Kill();
        animationTween = CreateTween();

        string lProperty = (string)Node2D.PropertyName.Position;

        Vector2 target = pIsOut ? shellPosOut : shellPosIn;
        Tween.EaseType ease = pIsOut ? Tween.EaseType.Out : Tween.EaseType.In;
        float duration = pIsOut ? headOutDuration : headInDuration;

        animationTween.TweenProperty(shell, lProperty, target, duration)
                      .SetTrans(Tween.TransitionType.Back)
                      .SetEase(ease);
    }

    private void PerformChargeAnimation()
    {
        Vector2 lRecoilDirection = -targetDashDirection;
        Vector2 lRecoilPos = GlobalPosition + (lRecoilDirection * recoilDistance);

        animationTween?.Kill();
        animationTween = CreateTween();

        animationTween.Parallel().TweenProperty(this, (string)Node2D.PropertyName.GlobalPosition, lRecoilPos, preDashDuration)
                      .SetTrans(Tween.TransitionType.Back)
                      .SetEase(Tween.EaseType.Out);

        animationTween.Parallel().TweenProperty(shell, (string)Node2D.PropertyName.Position, shellPosIn, preDashDuration)
                      .SetTrans(Tween.TransitionType.Back)
                      .SetEase(Tween.EaseType.Out);
    }

    public override void EntityDeath()
    {
        stateTimer?.Stop();
        animationTween?.Kill();
        base.EntityDeath();
    }
}