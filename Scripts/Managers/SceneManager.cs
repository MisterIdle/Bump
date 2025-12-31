using Godot;

public partial class SceneManager : Node
{
    private static SceneManager instance;
    public static SceneManager GetInstance()
    {
        return instance;
    }

    public enum SceneType
    {
        MainMenu,
        Game
    }

    [Export] private PackedScene mainMenuScene;
    [Export] private PackedScene gameScene;

    [Export] private Label debug;

    private Node currentScene;
    private SceneType currentSceneType;

    private Timer transitionTimer;
    private SceneType sceneToLoad;
    private bool isTransitioning = false;
    private float loadTime = 0.3f;

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else
        {
            QueueFree();
            return;
        }

        transitionTimer = new Timer();
        transitionTimer.OneShot = true;
        transitionTimer.WaitTime = loadTime;
        transitionTimer.Timeout += OnTransitionFinished;

        AddChild(transitionTimer);

        SwitchScene(SceneType.MainMenu);
    }

    public override void _Input(InputEvent pInput)
    {
        if (pInput.IsActionPressed(Property.INPUT_DEBUG))
            debug.Visible = !debug.Visible;
    }

    public override void _Process(double pDelta)
    {
        if (!debug.Visible) return;

        string lText =
            $"Scene : {currentSceneType}\n" +
            $"FPS : {Engine.GetFramesPerSecond()}\n" +
            $"Time : {Engine.TimeScale:F2}s\n" +
            $"Freeze Time : {Time.currentTimeScale:F2}s\n";

        Player lPlayer = Player.GetInstance();
        if (lPlayer != null)
            lText += $"Player State : {lPlayer.currentState}\n";

        Hammer lHammer = Hammer.GetInstance();
        if (lHammer != null)
            lText += $"Hammer State : {lHammer.currentState}\n";

        debug.Text = lText;
    }

    public void SwitchScene(SceneType pSceneType)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        sceneToLoad = pSceneType;

        if (currentScene != null)
        {
            RemoveChild(currentScene);
            currentScene.QueueFree();
            currentScene = null;
        }

        transitionTimer.Start();
    }

    public void ChangeScene(SceneType pNextScene)
    {
        if (isTransitioning) return;

        if (currentSceneType != SceneType.Game)
        {
            SwitchScene(pNextScene);
            return;
        }

        isTransitioning = true;
        float duration = 1.5f;

        GameManager gm = GameManager.GetInstance();
        if (gm != null)
        {
            gm.scrollSpeed = 0f;
            GameManager.scrollSpeedVelocity = Vector2.Zero;
        }

        Camera cam = Camera.GetInstance();
        if (cam != null)
        {
            Tween tween = CreateTween();
            tween.SetPauseMode(Tween.TweenPauseMode.Process);
            tween.TweenProperty(
                cam,
                "global_position",
                Vector2.Zero,
                duration
            ).SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.InOut);
        }

        Player player = Player.GetInstance();
        if (player != null)
            player.AnimatedDespawn(duration);

        HUD hud = HUD.GetInstance();
        if (hud != null)
            hud.PlayExitAnimation(duration);

        Timer timer = new Timer();
        timer.WaitTime = duration;
        timer.OneShot = true;
        AddChild(timer);

        timer.Timeout += () =>
        {
            isTransitioning = false;
            timer.QueueFree();
            SwitchScene(pNextScene);
        };

        timer.Start();
    }


    public void ReloadScene()
    {
        ChangeScene(SceneType.Game);
    }

    public void GoToMainMenu()
    {
        ChangeScene(SceneType.MainMenu);
    }

    private void OnTransitionFinished()
    {
        PackedScene lTargetScene = null;

        switch (sceneToLoad)
        {
            case SceneType.MainMenu:
                lTargetScene = mainMenuScene;
                break;
            case SceneType.Game:
                lTargetScene = gameScene;
                break;
        }

        if (lTargetScene != null)
        {
            currentScene = lTargetScene.Instantiate();
            AddChild(currentScene);
            currentSceneType = sceneToLoad;
        }

        isTransitioning = false;
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}