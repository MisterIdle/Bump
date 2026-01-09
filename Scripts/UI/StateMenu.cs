using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;
using Com.IsartDigital.Shmup.NManager;
using Godot;

namespace Com.IsartDigital.Shmup.NHUD;

public partial class StateMenu : MenuBase
{
    [Export] private Panel panel;
    [Export] private Label titleLabel;

    [ExportCategory("Buttons")]
    [Export] private Button resumeButton;
    [Export] private Button restartButton;
    [Export] private Button optionButton;
    [Export] private Button mainMenuButton;

    [ExportCategory("Background Settings")]
    [Export] public Color pauseColor = new Color(1, 1, 1, 1);
    [Export] public Color gameOverColor = new Color(0, 0, 0, 1);
    [Export] public Color victoryColor = new Color(1, 1, 1, 1);

    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        panel.Modulate = new Color(1, 1, 1, 0);

        optionButton.Pressed += OnOptionPressed;
        resumeButton.Pressed += OnResume;
        restartButton.Pressed += OnRestart;
        mainMenuButton.Pressed += OnMainMenu;

        Visible = false;
    }

    // ========================================================================
    // INPUT
    // ========================================================================

    public override void _Input(InputEvent pEvent)
    {
        if (pEvent.IsActionPressed(Property.INPUT_UI_PAUSE) && !MenuManager.GetInstance().loading)
        {
            MenuManager lManager = MenuManager.GetInstance();

            if (Visible && lManager.currentMenuState == MenuManager.MenuState.Pause)
                OnResume();

            else if (lManager.currentMenuState == MenuManager.MenuState.HUD)
                lManager.SetMenu(MenuManager.MenuState.Pause);
        }
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    public override void OnEnter()
    {
        Configure();
        GetTree().Paused = true;

        MenuManager lManager = MenuManager.GetInstance();

        lManager.previousMenuState = lManager.currentMenuState;

        if (panel.Modulate.A < 0.5f)
        {
            panel.Modulate = new Color(1, 1, 1, 0);
            CreateTween().TweenProperty(panel, "modulate:a", 1.0f, 0.4f);
        }
        else
            panel.Modulate = new Color(1, 1, 1, 1);

        base.OnEnter();

        if (resumeButton.Visible) resumeButton.GrabFocus();
        else restartButton.GrabFocus();

        AudioManager.GetInstance().SetMusicPaused(true);
    }

    public override void OnExit()
    {
        MenuManager lManager = MenuManager.GetInstance();

        if (lManager.previousMenuState == lManager.currentMenuState)
            CreateTween().TweenProperty(panel, "modulate:a", 0.0f, 0.4f);

        PlayGenericExitAnimation();
        base.OnExit();

        AudioManager.GetInstance().SetMusicPaused(false);
    }

    // ========================================================================
    // STATE
    // ========================================================================

    private void Configure()
    {
        MenuManager.MenuState lState = MenuManager.GetInstance().currentMenuState;

        resumeButton.Visible = false;
        restartButton.Visible = false;
        optionButton.Visible = false;
        mainMenuButton.Visible = false;

        switch (lState)
        {
            case MenuManager.MenuState.Pause:
                titleLabel.Text = "ID_PAUSE";
                resumeButton.Visible = true;
                restartButton.Visible = true;
                optionButton.Visible = true;
                mainMenuButton.Visible = true;
                panel.SelfModulate = pauseColor;
                break;

            case MenuManager.MenuState.GameOver:
                titleLabel.Text = "ID_GAMEOVER";
                restartButton.Visible = true;
                mainMenuButton.Visible = true;
                panel.SelfModulate = gameOverColor;
                break;
        }
    }

    // ========================================================================
    // ACTION
    // ========================================================================

    private void OnOptionPressed()
    {
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.Options);
        CreateTween().TweenProperty(panel, "modulate:a", 1.0f, 0.4f);
    }

    public void OnResume()
    {
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.HUD);
        GetTree().Paused = false;
    }

    public void OnRestart()
    {
        panel.MouseFilter = MouseFilterEnum.Ignore;

        CreateTween().TweenProperty(panel, "modulate:a", 0.0f, 0.4f);
        Tween lTween = PlayGenericExitAnimation();

        Enemy.ClearAll(true, true);
        Projectiles.ClearAll();

        lTween.Finished += () =>
        {
            GetTree().Paused = false;
            MenuManager.GetInstance().nextIsGame = true;
            SceneManager.GetInstance().ChangeGlobalScene(SceneManager.SceneType.MainMenu);
            MenuManager.GetInstance().SetMenu(MenuManager.MenuState.Help);
        };

        AudioManager.GetInstance().StopMusic();
    }

    public void OnMainMenu()
    {
        CreateTween().TweenProperty(panel, "modulate:a", 0.0f, 0.4f);
        Tween lTween = PlayGenericExitAnimation();

        Enemy.ClearAll(true, true);
        Projectiles.ClearAll();

        GetTree().Paused = false;
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.MainMenu);
        SceneManager.GetInstance().ChangeGlobalScene(SceneManager.SceneType.MainMenu);
        AudioManager.GetInstance().StopMusic();
    }
}