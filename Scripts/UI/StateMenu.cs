using Godot;

public partial class StateMenu : Control
{
    public enum MenuState
    {
        Pause,
        GameOver,
        Victory
    }

    private static StateMenu instance;

    public static StateMenu GetInstance()
    {
        return instance;
    }

    [Export] private MenuManager manager;
    [Export] private Panel panel;

    [Export] private Label titleLabel;

    [Export] private Button resumeButton;
    [Export] private Button restartButton;
    [Export] private Button optionButton;
    [Export] private Button mainMenuButton;

    private Tween currentTween;
    private MenuState currentState = MenuState.Pause;

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else
        {
            QueueFree();
            return;
        }

        ProcessMode = ProcessModeEnum.Always;

        Visible = false;
        panel.Modulate = new Color(1, 1, 1, 0);

        optionButton.Pressed += OnOptionPressed;
        resumeButton.Pressed += OnResume;
        restartButton.Pressed += OnRestart;
        mainMenuButton.Pressed += OnMainMenu;
    }

    public override void _Input(InputEvent pEvent)
    {
        if (pEvent.IsActionPressed(Property.INPUT_UI_PAUSE))
        {
            if (Visible)
            {
                if (manager.isShowingOptions)
                {
                    manager.ToggleMenu();
                    return;
                }

                if (currentState == MenuState.Pause)
                {
                    OnResume();
                }
            }
            else
            {
                OpenMenu(MenuState.Pause);
            }
        }
    }

    public void OpenMenu(MenuState state)
    {
        currentState = state;
        ConfigureUI();

        GetTree().Paused = true;
        Visible = true;
        PlayOpenAnimation();
    }

    private void ConfigureUI()
    {
        switch (currentState)
        {
            case MenuState.Pause:
                titleLabel.Text = "UI_PAUSE";
                resumeButton.Visible = true;
                restartButton.Visible = true;
                break;

            case MenuState.GameOver:
                titleLabel.Text = "UI_GAMEOVER";
                resumeButton.Visible = false;
                restartButton.Visible = true;
                break;

            case MenuState.Victory:
                titleLabel.Text = "UI_VICTORY";
                resumeButton.Visible = false;
                restartButton.Visible = true;
                break;
        }
    }

    private void OnOptionPressed()
    {
        manager.ToggleMenu();
    }

    public void OnResume()
    {
        if (currentState != MenuState.Pause) return;

        PlayCloseAnimation();
    }

    public void OnRestart()
    {
        Tween lTween = PlayCloseAnimation();
        lTween.Finished += () => SceneManager.GetInstance().ReloadScene();
    }

    public void OnMainMenu()
    {
        Tween lTween = PlayCloseAnimation();
        lTween.Finished += () => SceneManager.GetInstance().GoToMainMenu();
    }

    private void PlayOpenAnimation()
    {
        if (currentTween != null && currentTween.IsRunning()) currentTween.Kill();
        currentTween = CreateTween();
        currentTween.SetParallel(true);

        panel.Modulate = new Color(1, 1, 1, 0);
        currentTween.TweenProperty(panel, "modulate:a", 1.0f, 0.3f)
            .SetEase(Tween.EaseType.Out);

        float lCurrentDelay = 0.15f;
        AnimateButtonsInRecursive(this, currentTween, ref lCurrentDelay);

        currentTween.Finished += () =>
        {
            if (resumeButton.Visible)
                resumeButton.GrabFocus();
            else
                restartButton.GrabFocus();
        };
    }

    private void AnimateButtonsInRecursive(Node pNode, Tween pTween, ref float pDelay)
    {
        foreach (Node lChild in pNode.GetChildren())
        {
            if (lChild is Button lButton)
            {
                if (!lButton.Visible) continue;

                lButton.Modulate = new Color(1, 1, 1, 0);
                lButton.Scale = new Vector2(0f, 0f);
                lButton.PivotOffset = lButton.Size / 2;

                pTween.TweenProperty(lButton, "modulate:a", 1f, 0.2f)
                    .SetDelay(pDelay);
                pTween.TweenProperty(lButton, "scale", Vector2.One, 0.5f)
                    .SetDelay(pDelay)
                    .SetEase(Tween.EaseType.Out)
                    .SetTrans(Tween.TransitionType.Elastic);

                pDelay += 0.08f;
            }

            if (lChild.GetChildCount() > 0)
                AnimateButtonsInRecursive(lChild, pTween, ref pDelay);
        }
    }

    private Tween PlayCloseAnimation()
    {
        if (currentTween != null && currentTween.IsRunning()) currentTween.Kill();
        currentTween = CreateTween();
        currentTween.SetParallel(true);

        float delay = 0f;
        AnimateButtonsOutRecursive(this, currentTween, ref delay);

        currentTween.TweenProperty(panel, "modulate:a", 0.0f, 0.3f)
            .SetDelay(0.1f)
            .SetEase(Tween.EaseType.In);

        currentTween.Finished += () =>
        {
            Visible = false;
            GetTree().Paused = false;
        };

        return currentTween;
    }

    private void AnimateButtonsOutRecursive(Node pNode, Tween pTween, ref float pDelay)
    {
        foreach (Node lChild in pNode.GetChildren())
        {
            if (lChild is Button lButton)
            {
                if (!lButton.Visible) continue;

                lButton.PivotOffset = lButton.Size / 2;
                pTween.TweenProperty(lButton, "scale", Vector2.Zero, 0.2f)
                    .SetEase(Tween.EaseType.In)
                    .SetTrans(Tween.TransitionType.Back);
                pTween.TweenProperty(lButton, "modulate:a", 0f, 0.15f);
            }

            if (lChild.GetChildCount() > 0)
                AnimateButtonsOutRecursive(lChild, pTween, ref pDelay);
        }
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}