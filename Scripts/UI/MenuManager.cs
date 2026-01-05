using Godot;

namespace Com.IsartDigital.Shmup.NHUD;

public partial class MenuManager : Control
{
    private static MenuManager instance;
    public static MenuManager GetInstance()
    {
        return instance;
    }

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

    [Export] public Marker2D pivot;

    [ExportCategory("Menus")]
    [Export] public MenuBase MainMenu;
    [Export] public MenuBase OptionsMenu;
    [Export] public MenuBase HUDMenu;
    [Export] public MenuBase StateMenu;
    [Export] public Victory VictoryMenu;
    [Export] public MenuBase Credits;

    [ExportCategory("Settings")]
    [Export] public float FlipDuration = 0.6f;

    public MenuState currentMenuState = MenuState.None;
    public MenuState previousMenuState = MenuState.None;

    private MenuBase currentMenu = null;
    private Tween flipTween;

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
        OptionsMenu.Visible = false;
        HUDMenu.Visible = false;
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
        if (currentMenuState == pNewState) return;

        previousMenuState = currentMenuState;

        currentMenu?.OnExit();

        MenuBase lNextMenuNode = null;
        float lTargetRotation = 0f;

        switch (pNewState)
        {
            case MenuState.MainMenu:
                lNextMenuNode = MainMenu;
                break;

            case MenuState.Options:
                lNextMenuNode = OptionsMenu;
                lTargetRotation = 180f;
                break;

            case MenuState.Credits:
                lNextMenuNode = Credits;
                break;

            case MenuState.HUD:
                lNextMenuNode = HUDMenu;
                break;

            case MenuState.Victory:
                lNextMenuNode = VictoryMenu;
                break;

            case MenuState.Pause:
            case MenuState.GameOver:
                lNextMenuNode = StateMenu;
                lTargetRotation = 0f;
                break;
        }

        currentMenuState = pNewState;
        Flip(lTargetRotation);

        if (lNextMenuNode == null)
        {
            currentMenu = null;
            GetTree().Paused = false;
            return;
        }

        float lPrevTime = 0.2f;

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

    public void Flip(float pDegrees)
    {
        flipTween?.Kill();

        flipTween = CreateTween();
        flipTween.TweenProperty(pivot, (string)PropertyName.RotationDegrees, pDegrees, FlipDuration)
            .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Back);
    }

    protected override void Dispose(bool pDisposing)
    {
        if (instance == this) instance = null;
        base.Dispose(pDisposing);
    }
}