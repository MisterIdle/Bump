using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NHUD;
using Com.IsartDigital.Shmup.NManager;
using Godot;
using System;

namespace Com.IsartDigital.Shmup.NDebug;

public partial class Debug : Node2D
{
    [Export] private Marker2D tpMarker;

    private const float HALF_DIVIDER = 2f;
    private const float TELEPORT_DURATION = 1.0f;

    public bool inPause;

    public override void _Input(InputEvent pEvent)
    {
        if (pEvent.IsActionPressed(Property.INPUT_BOSS))
            TeleportCameraAndGodMode();
    }

    private void TeleportCameraAndGodMode()
    {
        Player.GetInstance().ToggleGodMode();

        Camera lCam = Camera.GetInstance();
        lCam.isScrolling = false;

        Vector2 lScreenSize = lCam.GetViewportRect().Size;
        Vector2 lTargetPos = tpMarker.GlobalPosition - (lScreenSize / HALF_DIVIDER);

        Tween lTween = CreateTween();

        lTween.TweenProperty(lCam, (string)Node2D.PropertyName.GlobalPosition, lTargetPos, TELEPORT_DURATION)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);

        lTween.Finished += () => lCam.isScrolling = true;
    }
}