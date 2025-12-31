using Godot;

public partial class MenuManager : Control
{
    public enum MenuContext
    {
        MainMenu,
        PauseMenu
    }

    [Export] private Node2D pivot;
    [Export] private OptionsMenu optionsMenu;
    [Export] private ColorRect stop;

    private Tween flipTween;
    public bool isShowingOptions = false;

    private Control lastFocusedControl;
    public MenuContext CurrentContext;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
    }

    public void ToggleMenu(MenuContext context = MenuContext.MainMenu)
    {
        if (flipTween != null && flipTween.IsRunning()) return;

        stop.Visible = true;

        if (!isShowingOptions)
        {
            lastFocusedControl = GetViewport().GuiGetFocusOwner();
            CurrentContext = context;

            GetViewport().GuiReleaseFocus();
        }

        isShowingOptions = !isShowingOptions;
        float lTargetAngle = isShowingOptions ? 180f : 0f;

        flipTween?.Kill();
        flipTween = CreateTween();

        flipTween.TweenProperty(pivot, (string)PropertyName.RotationDegrees, lTargetAngle, 0.6f)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Back);

        flipTween.Finished += () =>
        {
            if (isShowingOptions)
            {
                optionsMenu.OnMenuShow();
            }
            else
            {
                if (lastFocusedControl != null && IsInstanceValid(lastFocusedControl))
                {
                    lastFocusedControl.GrabFocus();
                }
            }

            stop.Visible = false;
        };
    }
}