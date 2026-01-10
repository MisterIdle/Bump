using Godot;

namespace Com.IsartDigital.Shmup.NToolsBox;

public partial class SwitchImage : TextureRect
{
    [ExportGroup("Keyboard Images")]
    [Export] private Texture2D keyboardIconA;
    [Export] private Texture2D keyboardIconB;

    [ExportGroup("Controller Images")]
    [Export] private Texture2D controllerIconA;
    [Export] private Texture2D controllerIconB;

    [ExportGroup("Settings")]
    [Export] private float blinkSpeed = DEFAULT_BLINK_SPEED;

    private Timer blinkTimer;
    private bool isUsingController = false;
    private bool showFirstFrame = true;

    private const float DEFAULT_BLINK_SPEED = 0.5f;
    private const float JOYPAD_DEADZONE = 0.2f;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        blinkTimer = new Timer();
        blinkTimer.WaitTime = blinkSpeed;
        blinkTimer.OneShot = false;
        blinkTimer.Autostart = true;

        blinkTimer.Timeout += OnTimerTimeout;

        AddChild(blinkTimer);

        UpdateTexture();
    }

    // ========================================================================
    // INPUT
    // ========================================================================

    public override void _Input(InputEvent pEvent)
    {
        if (pEvent is InputEventKey || pEvent is InputEventMouseButton)
        {
            if (isUsingController)
            {
                isUsingController = false;
                UpdateTexture();
                ResetBlinkCycle();
            }
        }
        else if (pEvent is InputEventJoypadButton || pEvent is InputEventJoypadMotion)
        {
            if (pEvent is InputEventJoypadMotion lMotion && Mathf.Abs(lMotion.AxisValue) < JOYPAD_DEADZONE)
                return;

            if (!isUsingController)
            {
                isUsingController = true;
                UpdateTexture();
                ResetBlinkCycle();
            }
        }
    }

    // ========================================================================
    // LOGIC
    // ========================================================================

    private void OnTimerTimeout()
    {
        showFirstFrame = !showFirstFrame;
        UpdateTexture();
    }

    private void ResetBlinkCycle()
    {
        showFirstFrame = true;
        blinkTimer.Start();
        UpdateTexture();
    }

    private void UpdateTexture()
    {
        if (isUsingController)
            Texture = showFirstFrame ? controllerIconA : controllerIconB;
        else
            Texture = showFirstFrame ? keyboardIconA : keyboardIconB;
    }
}