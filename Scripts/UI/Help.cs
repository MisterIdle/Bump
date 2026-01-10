using Com.IsartDigital.Shmup.NHUD;
using Com.IsartDigital.Shmup.NManager;
using Godot;
using System;

public partial class Help : MenuBase
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

        bool lIsAnyAction = pEvent.IsPressed() &&
                           (pEvent is InputEventKey ||
                            pEvent is InputEventMouseButton ||
                            pEvent is InputEventJoypadButton);

        if (lIsAnyAction && !MenuManager.GetInstance().loading)
        {
            GetViewport().SetInputAsHandled();
            PlayGenericEntryAnimation();

            MenuManager lManager = MenuManager.GetInstance();

            if (MenuManager.GetInstance().nextIsGame)
            {
                MenuManager.GetInstance().SetMenu(MenuManager.MenuState.None);
                SceneManager.GetInstance().ChangeGlobalScene(SceneManager.SceneType.Game);
                AudioManager.GetInstance().StopMusic();
                MenuManager.GetInstance().nextIsGame = false;
            }
            else
            {
                MenuManager.GetInstance().SetMenu(MenuManager.MenuState.MainMenu);
            }
        }
    }
}
