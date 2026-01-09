using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;
using Com.IsartDigital.Shmup.NToolsBox;
using Godot;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class Rushroops : ShelledEnemy
{
    [ExportGroup("Animation Settings")]
    [Export] private Node2D headVisuals;
    [Export] private float headHideOffset = 20f;
    [Export] private float headOutDuration = 0.4f;
    [Export] private float headInDuration = 0.3f;

    [ExportGroup("State Timings")]
    [Export] private float spawnDuration = 5.0f;
    [Export] private float shootDuration = 2.0f;
    [Export] private float preDashDuration = 0.8f;
    [Export] private float dashDuration = 0.4f;
    [Export] private float recoveryDuration = 1.0f;

    [ExportGroup("Physics Settings")]
    [Export] private float dashSpeed = 800f;
    [Export] private float recoilDistance = 50f;
    [Export] private float deceleration = 5f;

    private enum EnemyState { Spawn, Shooting, PreDash, Dashing, Recovery }
    private EnemyState currentState;
    private Timer stateTimer;

    private float defaultSpeed;
    private Tween animationTween;
    [Export] private Vector2 shellPosOut;
    [Export] private Vector2 shellPosIn;

    public override void Initiate()
    {
        base.Initiate();
        defaultSpeed = speed;
        baseScale = Scale;

        shell?.SetShellColor(Colors.Blue);

        stateTimer = new Timer();
        stateTimer.OneShot = true;
        stateTimer.Timeout += OnStateTimerTimeout;
        AddChild(stateTimer);

        ChangeState(EnemyState.Spawn);
    }

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        switch (currentState)
        {
            case EnemyState.Spawn:
                velocity.X = angleX;
                break;

            case EnemyState.Shooting:
                velocity.X = angleX;
                break;

            case EnemyState.PreDash:
                velocity = Vector2.Zero;
                break;

            case EnemyState.Dashing:
                break;

            case EnemyState.Recovery:
                velocity = velocity.Lerp(Vector2.Zero, deceleration * pDelta);
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
                shellProjectileProtect = true;

                shell.Position = shellPosOut;

                stateTimer.Start(spawnDuration);
                break;

            case EnemyState.Shooting:
                speed = defaultSpeed;
                shellProjectileProtect = false;
                wing?.SetSpeedDouble(false);

                AnimateHead(true);
                StartSpike();
                stateTimer.Start(shootDuration);
                break;

            case EnemyState.PreDash:
                StopSpike();
                shellProjectileProtect = true;
                shell?.SetShellColor(Colors.DarkBlue);
                wing?.SetSpeedDouble(true);
                PerformChargeAnimation();

                stateTimer.Start(preDashDuration);
                break;

            case EnemyState.Dashing:
                speed = dashSpeed;
                velocity = playerDirection.Normalized();

                stateTimer.Start(dashDuration);
                break;

            case EnemyState.Recovery:
                wing?.SetSpeedDouble(false);
                shell?.SetShellColor(Colors.Blue);

                stateTimer.Start(recoveryDuration);
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

            case EnemyState.Dashing:
                ChangeState(EnemyState.Recovery);
                break;

            case EnemyState.Recovery:
                ChangeState(EnemyState.Shooting);
                break;
        }
    }

    private void AnimateHead(bool pIsOut)
    {
        animationTween?.Kill();
        animationTween = CreateTween();

        if (pIsOut)
        {
            animationTween.TweenProperty(shell, "position", shellPosOut, headOutDuration)
                          .SetTrans(Tween.TransitionType.Back)
                          .SetEase(Tween.EaseType.Out);
        }
        else
        {
            animationTween.TweenProperty(shell, "position", shellPosIn, headInDuration)
                          .SetTrans(Tween.TransitionType.Back)
                          .SetEase(Tween.EaseType.In);
        }
    }

    private void PerformChargeAnimation()
    {
        Vector2 lRecoilPos = GlobalPosition - (playerDirection.Normalized() * recoilDistance);

        animationTween?.Kill();
        animationTween = CreateTween();

        animationTween.Parallel().TweenProperty(this, "global_position", lRecoilPos, preDashDuration)
                      .SetTrans(Tween.TransitionType.Back)
                      .SetEase(Tween.EaseType.Out);

        animationTween.Parallel().TweenProperty(shell, "position", shellPosIn, preDashDuration)
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