using Godot;
using System;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NDebug;

public partial class Debug : Node2D
{
    [Export] private Marker2D tp;

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
        Vector2 lTargetPos = tp.GlobalPosition - (lScreenSize / 2f);

        Tween lTween = CreateTween();

        lTween.TweenProperty(lCam, (string)Node2D.PropertyName.GlobalPosition, lTargetPos, 1.0f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);

        lTween.Finished += () => lCam.isScrolling = true;
    }
}