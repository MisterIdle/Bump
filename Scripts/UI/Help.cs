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

        if (pEvent.IsActionPressed(Property.INPUT_UI_PAUSE))
        {
            GetViewport().SetInputAsHandled();
            PlayGenericEntryAnimation();

            MenuManager lManager = MenuManager.GetInstance();

            if (MenuManager.GetInstance().nextIsGame)
            {
                MenuManager.GetInstance().nextIsGame = false;
                MenuManager.GetInstance().SetMenu(MenuManager.MenuState.None);
                SceneManager.GetInstance().ChangeScene(SceneManager.SceneType.Game);

            } 
            else
            {
                MenuManager.GetInstance().SetMenu(MenuManager.MenuState.MainMenu);
            }
        }
    }
}
