using Godot;

using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NHUD;

public partial class OptionsMenu : MenuBase
{
    [Export] private MenuManager manager;

    [ExportGroup("References")]
    [Export] private Button backButton;
    [Export] private Button franceButton;
    [Export] private Button englishButton;
    [Export] private CheckBox fullscreenCheck;
    [Export] private CheckBox vsyncCheck;

    [ExportGroup("Audio Settings")]
    [Export] private HSlider musicSlider;
    [Export] private HSlider sfxSlider;

    [ExportGroup("Lang")]
    private const string FRANCE = "fr";
    private const string USA = "en";

    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        backButton.Pressed += OnBackPressed;
        franceButton.Pressed += () => ChangeLocale(FRANCE);
        englishButton.Pressed += () => ChangeLocale(USA);

        SetupVideoSettings();
        SetupAudioSettings();
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    public override void OnEnter()
    {
        base.OnEnter();
        backButton.GrabFocus();
    }

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
        if (Visible && pEvent.IsActionPressed(Property.INPUT_UI_BACK) && !MenuManager.GetInstance().loading)
            OnBackPressed();
    }

    // ========================================================================
    // SETUP / CHANGE PARAMETERS
    // ========================================================================

    private void SetupVideoSettings()
    {
        if (fullscreenCheck == null || vsyncCheck == null) return;

        bool lIsFullscreen = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.ExclusiveFullscreen || DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
        fullscreenCheck.ButtonPressed = lIsFullscreen;
        fullscreenCheck.Toggled += OnFullscreenToggled;

        bool isVsync = DisplayServer.WindowGetVsyncMode() == DisplayServer.VSyncMode.Enabled;
        vsyncCheck.ButtonPressed = isVsync;
        vsyncCheck.Toggled += OnVsyncToggled;
    }

    private void SetupAudioSettings()
    {
        musicSlider.Value = GetBusVolumeLinear("Music");
        sfxSlider.Value = GetBusVolumeLinear("SFX");

        musicSlider.ValueChanged += (value) => OnVolumeSliderChanged(value, "Music");
        sfxSlider.ValueChanged += (value) => OnVolumeSliderChanged(value, "SFX");
    }

    private void OnVolumeSliderChanged(double value, string busName)
    {
        float lDB = (float)Mathf.LinearToDb(value);
        int lIndex = AudioServer.GetBusIndex(busName);

        if (lIndex != -1)
        {
            AudioServer.SetBusVolumeDb(lIndex, lDB);
            AudioServer.SetBusMute(lIndex, value <= musicSlider.MinValue);
        }

        if (busName == "SFX")
            AudioManager.GetInstance().Play(AudioManager.AudioType.UIClic);
    }

    private float GetBusVolumeLinear(string busName)
    {
        int busIndex = AudioServer.GetBusIndex(busName);
        if (busIndex != -1)
        {
            return (float)Mathf.DbToLinear(AudioServer.GetBusVolumeDb(busIndex));
        }
        return 1.0f;
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
        MenuManager.GetInstance().ReturnFromSubMenu();
    }
}
