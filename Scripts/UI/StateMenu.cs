using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NToolsBox;
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
    [Export] public Color pauseColor = Colors.White;
    [Export] public Color gameOverColor = Colors.Black;
    [Export] public Color victoryColor = Colors.White;

    private const string TEXT_ID_PAUSE = "ID_PAUSE";
    private const string TEXT_ID_GAMEOVER = "ID_GAMEOVER";

    private const float ALPHA_TRANSPARENT = 0.0f;
    private const float ALPHA_OPAQUE = 1.0f;
    private const float ALPHA_THRESHOLD = 0.5f;

    private const float FADE_DURATION = 0.4f;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        panel.Modulate = Colors.Transparent;

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

        AudioManager.GetInstance().Play(AudioManager.AudioType.UIPause);

        MenuManager lManager = MenuManager.GetInstance();

        lManager.previousMenuState = lManager.currentMenuState;

        if (panel.Modulate.A < ALPHA_THRESHOLD)
        {
            panel.Modulate = Colors.Transparent;
            CreateTween().TweenProperty(panel, (string)PropertyName.Modulate + ":a", ALPHA_OPAQUE, FADE_DURATION);
        }
        else
            panel.Modulate = Colors.White;

        base.OnEnter();

        if (resumeButton.Visible)
            resumeButton.GrabFocus();
        else
            restartButton.GrabFocus();

        AudioManager.GetInstance().SetMusicPaused(true);
    }

    public override void OnExit()
    {
        MenuManager lManager = MenuManager.GetInstance();

        if (lManager.previousMenuState == lManager.currentMenuState)
            CreateTween().TweenProperty(panel, (string)PropertyName.Modulate + ":a", ALPHA_TRANSPARENT, FADE_DURATION);

        PlayGenericExitAnimation();
        base.OnExit();
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
                titleLabel.Text = TEXT_ID_PAUSE;
                resumeButton.Visible = true;
                restartButton.Visible = true;
                optionButton.Visible = true;
                mainMenuButton.Visible = true;
                panel.SelfModulate = pauseColor;
                break;

            case MenuManager.MenuState.GameOver:
                titleLabel.Text = TEXT_ID_GAMEOVER;
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
        CreateTween().TweenProperty(panel, (string)PropertyName.Modulate + ":a", ALPHA_OPAQUE, FADE_DURATION);
        AudioManager.GetInstance().Play(AudioManager.AudioType.UIClic);
    }

    public void OnResume()
    {
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.HUD);
        GetTree().Paused = false;
        AudioManager.GetInstance().Play(AudioManager.AudioType.UIClic);
    }

    public void OnRestart()
    {
        panel.MouseFilter = MouseFilterEnum.Ignore;

        CreateTween().TweenProperty(panel, (string)PropertyName.Modulate + ":a", ALPHA_TRANSPARENT, FADE_DURATION);
        Tween lTween = PlayGenericExitAnimation();

        Enemy.ClearAll(true, true);
        Projectiles.ClearAll();

        lTween.Finished += RestartSwitch;

        AudioManager.GetInstance().Play(AudioManager.AudioType.UIClic);
        AudioManager.GetInstance().StopMusic();
    }

    private void RestartSwitch()
    {
        GetTree().Paused = false;

        HUD.GetInstance().ResetHUD();
        MenuManager.GetInstance().nextIsGame = true;
        SceneManager.GetInstance().ChangeGlobalScene(SceneManager.SceneType.MainMenu);
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.Help);
        AudioManager.GetInstance().Play(AudioManager.AudioType.UIClic);
    }

    public void OnMainMenu()
    {
        CreateTween().TweenProperty(panel, (string)PropertyName.Modulate + ":a", ALPHA_TRANSPARENT, FADE_DURATION);
        Tween lTween = PlayGenericExitAnimation();

        Enemy.ClearAll(true, true);
        Projectiles.ClearAll();

        GetTree().Paused = false;
        MenuManager.GetInstance().SetMenu(MenuManager.MenuState.MainMenu);
        SceneManager.GetInstance().ChangeGlobalScene(SceneManager.SceneType.MainMenu);
        AudioManager.GetInstance().Play(AudioManager.AudioType.UIClic);
    }
}