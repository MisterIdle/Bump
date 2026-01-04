using Godot;

public partial class Credits : MenuBase
{
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

    public override void OnExit()
    {
        PlayGenericExitAnimation();
    }
}