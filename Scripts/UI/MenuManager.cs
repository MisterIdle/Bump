using Godot;

namespace Com.IsartDigital.Shmup.NHUD;

public partial class MenuManager : Control
{
    public enum MenuState
    {
        None,
        MainMenu,
        Options,
        HUD,
        Pause,
        Victory,
        GameOver,
        Help,
        Credits
    }

    [ExportCategory("Menus")]
    [Export] public MenuBase MainMenu;
    [Export] public MenuBase Options;
    [Export] public MenuBase HUD;
    [Export] public MenuBase Help;
    [Export] public MenuBase StateMenu;
    [Export] public Victory VictoryMenu;
    [Export] public MenuBase Credits;

    public bool nextIsGame = false;
    public bool loading;

    [ExportCategory("Settings")]
    [Export] public float FlipDuration = 0.6f;

    public MenuState currentMenuState = MenuState.None;
    public MenuState previousMenuState = MenuState.None;

    private MenuBase currentMenu = null;
    private Tween flipTween;

    private const float MENU_TRANSITION_DELAY = 0.2f;

    private static MenuManager instance;
    public static MenuManager GetInstance()
    {
        return instance;
    }

    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else
        {
            QueueFree();
            return;
        }

        MainMenu.Visible = false;
        Options.Visible = false;
        HUD.Visible = false;
        Help.Visible = false;
        StateMenu.Visible = false;
        VictoryMenu.Visible = false;
        Credits.Visible = false;

        SetMenu(MenuState.MainMenu);
    }

    // ========================================================================
    // STATE
    // ========================================================================

    public void SetMenu(MenuState pNewState)
    {
        if (!loading) loading = true;

        if (currentMenuState == pNewState) return;

        previousMenuState = currentMenuState;

        currentMenu?.OnExit();

        MenuBase lNextMenuNode = null;

        switch (pNewState)
        {
            case MenuState.MainMenu:
                lNextMenuNode = MainMenu;
                break;

            case MenuState.Options:
                lNextMenuNode = Options;
                break;

            case MenuState.Credits:
                lNextMenuNode = Credits;
                break;

            case MenuState.HUD:
                lNextMenuNode = HUD;
                break;

            case MenuState.Help:
                lNextMenuNode = Help;
                break;

            case MenuState.Victory:
                lNextMenuNode = VictoryMenu;
                break;

            case MenuState.Pause:
            case MenuState.GameOver:
                lNextMenuNode = StateMenu;
                break;
        }

        currentMenuState = pNewState;

        if (lNextMenuNode == null)
        {
            currentMenu = null;
            GetTree().Paused = false;
            return;
        }

        float lPrevTime = MENU_TRANSITION_DELAY;

        Timer lTimer = new Timer();
        lTimer.OneShot = true;
        lTimer.WaitTime = lPrevTime;
        AddChild(lTimer);

        lTimer.Timeout += () =>
        {
            if (currentMenuState != pNewState) return;

            currentMenu = lNextMenuNode;
            currentMenu.OnEnter();
            lTimer.QueueFree();
        };

        lTimer.Start();
    }

    public void ReturnFromSubMenu()
    {
        if (previousMenuState != MenuState.None)
            SetMenu(previousMenuState);
        else
            SetMenu(MenuState.MainMenu);
    }

    // ========================================================================
    // ANIMATION
    // ========================================================================

    protected override void Dispose(bool pDisposing)
    {
        if (instance == this) instance = null;
        base.Dispose(pDisposing);
    }
}