using Godot;

using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NHUD;

public partial class Credits : MenuBase
{
    public override void OnExit()
    {
        PlayGenericExitAnimation();
    }

    // ========================================================================
    // INPUT
    // ========================================================================

    public override void _Input(InputEvent pEvent)
    {
        if (!Visible) return;

        if (pEvent.IsActionPressed(Property.INPUT_UI_PAUSE))
        {
            GetViewport().SetInputAsHandled();

            MenuManager lManager = MenuManager.GetInstance();

            if (lManager.previousMenuState == MenuManager.MenuState.MainMenu || lManager.previousMenuState == MenuManager.MenuState.None)
                lManager.SetMenu(MenuManager.MenuState.MainMenu);
            else
            {
                lManager.SetMenu(MenuManager.MenuState.MainMenu);
                SceneManager.GetInstance().ChangeScene(SceneManager.SceneType.MainMenu);
            }
        }
    }
}