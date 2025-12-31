using Godot;

public partial class OptionsMenu : Control
{
    [Export] private MenuManager manager;

    [Export] private Button backButton;

    [Export] private Button franceButton;
    [Export] private Button englishButton;

    [Export] private CheckBox fullscreenCheck;
    [Export] private CheckBox vsyncCheck;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        backButton.Pressed += OnBackPressed;

        franceButton.Pressed += () => ChangeLocale("fr");
        englishButton.Pressed += () => ChangeLocale("en");

        SetupVideoSettings();
    }

    public override void _Input(InputEvent pEvent)
    {
        if (pEvent.IsActionPressed(Property.INPUT_UI_BACK) && manager.isShowingOptions)
            OnBackPressed();
    }

    private void SetupVideoSettings()
    {
        bool lIsFullscreen = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.ExclusiveFullscreen || DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;

        fullscreenCheck.ButtonPressed = lIsFullscreen;
        fullscreenCheck.Toggled += OnFullscreenToggled;

        bool isVsync = DisplayServer.WindowGetVsyncMode() == DisplayServer.VSyncMode.Enabled;
        vsyncCheck.ButtonPressed = isVsync;
        vsyncCheck.Toggled += OnVsyncToggled;
    }

    private void OnFullscreenToggled(bool pIsCheck)
    {
        if (pIsCheck)
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        else
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Maximized);
    }

    private void OnVsyncToggled(bool pIsCheck)
    {
        DisplayServer.WindowSetVsyncMode(pIsCheck ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
    }

    private void ChangeLocale(string localeCode)
    {
        TranslationServer.SetLocale(localeCode);
    }

    private void OnBackPressed()
    {
        manager.ToggleMenu();
    }

    public void OnMenuShow()
    {
        backButton.GrabFocus();
    }
}