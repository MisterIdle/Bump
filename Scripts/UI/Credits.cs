using Godot;

namespace Com.IsartDigital.Shmup.NHUD;

public partial class Credits : MenuBase
{
    public override void OnExit()
    {
        base.OnExit();
        PlayGenericExitAnimation();
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

            MenuManager lManager = MenuManager.GetInstance();
            lManager.SetMenu(MenuManager.MenuState.MainMenu);
        }
    }
}