using Godot;

namespace Com.IsartDigital.Shmup.NToolsBox;

public partial class Sign : Control
{
    public enum AnimationMode
    {
        OpacityFlash,
        ImageSequence
    }

    [ExportGroup("References")]
    [Export] private TextureRect targetImage;
    [Export] private Label targetLabel;

    [Export] private string idLabel;

    [ExportGroup("Configuration")]
    [Export] private AnimationMode currentMode = AnimationMode.OpacityFlash;
    [Export] private float blinkSpeed = 0.5f;
    [Export(PropertyHint.Range, "0,1")] private float minOpacity = 0.3f;

    [ExportGroup("Keyboard Data")]
    [Export] private Godot.Collections.Array<Texture2D> keyboardTextures;

    [ExportGroup("Controller Data")]
    [Export] private Godot.Collections.Array<Texture2D> controllerTextures;

    private Timer blinkTimer;
    private bool isUsingController = false;

    private int currentFrameIndex = 0;
    private bool isHighOpacity = true;

    private const float JOYPAD_DEADZONE = 0.2f;

    public override void _Ready()
    {
        if (keyboardTextures == null) keyboardTextures = new();
        if (controllerTextures == null) controllerTextures = new();

        targetLabel.Text = idLabel;

        blinkTimer = new Timer();
        blinkTimer.WaitTime = blinkSpeed;
        blinkTimer.OneShot = false;
        blinkTimer.Autostart = true;
        blinkTimer.Timeout += OnTimerTimeout;

        AddChild(blinkTimer);

        UpdateVisuals();
    }

    public override void _Input(InputEvent pEvent)
    {
        if (pEvent is InputEventKey || pEvent is InputEventMouseButton)
        {
            if (isUsingController)
            {
                ChangeInputMethod(false);
            }
        }
        else if (pEvent is InputEventJoypadButton || pEvent is InputEventJoypadMotion)
        {
            if (pEvent is InputEventJoypadMotion lMotion && Mathf.Abs(lMotion.AxisValue) < JOYPAD_DEADZONE)
                return;

            if (!isUsingController)
            {
                ChangeInputMethod(true);
            }
        }
    }

    private void ChangeInputMethod(bool pIsController)
    {
        isUsingController = pIsController;
        ResetBlinkCycle();
    }

    private void OnTimerTimeout()
    {
        currentFrameIndex++;
        isHighOpacity = !isHighOpacity;

        UpdateVisuals();
    }

    private void ResetBlinkCycle()
    {
        currentFrameIndex = 0;
        isHighOpacity = true;
        blinkTimer.Start();
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (targetImage == null) return;

        var currentList = isUsingController ? controllerTextures : keyboardTextures;

        if (currentList == null || currentList.Count == 0) return;

        if (currentMode == AnimationMode.ImageSequence)
        {
            targetImage.Modulate = new Color(1, 1, 1, 1);
            int safeIndex = currentFrameIndex % currentList.Count;
            targetImage.Texture = currentList[safeIndex];
        }
        else
        {
            targetImage.Texture = currentList[0];
            float alpha = isHighOpacity ? 1.0f : minOpacity;
            targetImage.Modulate = new Color(1, 1, 1, alpha);
        }
    }
}