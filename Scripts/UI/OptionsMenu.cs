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

    private const string LOCALE_FR = "fr";
    private const string LOCALE_US = "en";

    private const string BUS_MUSIC = "Music";
    private const string BUS_SFX = "SFX";

    private const float DEFAULT_VOLUME_LINEAR = 1.0f;
    private const int INVALID_BUS_INDEX = -1;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        backButton.Pressed += OnBackPressed;
        franceButton.Pressed += () => ChangeLocale(LOCALE_FR);
        englishButton.Pressed += () => ChangeLocale(LOCALE_US);

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

        bool lIsFullscreen = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.ExclusiveFullscreen ||
                             DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;

        fullscreenCheck.ButtonPressed = lIsFullscreen;
        fullscreenCheck.Toggled += OnFullscreenToggled;

        bool lIsVsync = DisplayServer.WindowGetVsyncMode() == DisplayServer.VSyncMode.Enabled;
        vsyncCheck.ButtonPressed = lIsVsync;
        vsyncCheck.Toggled += OnVsyncToggled;
    }

    private void SetupAudioSettings()
    {
        musicSlider.Value = GetBusVolumeLinear(BUS_MUSIC);
        sfxSlider.Value = GetBusVolumeLinear(BUS_SFX);

        musicSlider.ValueChanged += (pValue) => OnVolumeSliderChanged(pValue, BUS_MUSIC);
        sfxSlider.ValueChanged += (pValue) => OnVolumeSliderChanged(pValue, BUS_SFX);
    }

    private void OnVolumeSliderChanged(double pValue, string pBusName)
    {
        float lDB = (float)Mathf.LinearToDb(pValue);
        int lIndex = AudioServer.GetBusIndex(pBusName);

        if (lIndex != INVALID_BUS_INDEX)
        {
            AudioServer.SetBusVolumeDb(lIndex, lDB);
            AudioServer.SetBusMute(lIndex, pValue <= musicSlider.MinValue);
        }

        if (pBusName == BUS_SFX)
            AudioManager.GetInstance().Play(AudioManager.AudioType.uiClick);
    }

    private float GetBusVolumeLinear(string pBusName)
    {
        int lBusIndex = AudioServer.GetBusIndex(pBusName);
        if (lBusIndex != INVALID_BUS_INDEX)
        {
            return (float)Mathf.DbToLinear(AudioServer.GetBusVolumeDb(lBusIndex));
        }
        return DEFAULT_VOLUME_LINEAR;
    }

    private void OnFullscreenToggled(bool pIsCheck)
    {
        if (pIsCheck)
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        else
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Maximized);

        AudioManager.GetInstance().Play(AudioManager.AudioType.uiClick);
    }

    private void OnVsyncToggled(bool pIsCheck)
    {
        DisplayServer.WindowSetVsyncMode(pIsCheck ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
        AudioManager.GetInstance().Play(AudioManager.AudioType.uiClick);
    }

    private void ChangeLocale(string pLocaleCode)
    {
        TranslationServer.SetLocale(pLocaleCode);
        AudioManager.GetInstance().Play(AudioManager.AudioType.uiClick);
    }

    private void OnBackPressed()
    {
        MenuManager.GetInstance().ReturnFromSubMenu();
        AudioManager.GetInstance().Play(AudioManager.AudioType.uiClick);
    }
}