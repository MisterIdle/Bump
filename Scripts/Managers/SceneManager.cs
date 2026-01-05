using Godot;

using Com.IsartDigital.Shmup.NHUD;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NManager;

public partial class SceneManager : Node
{
    private static SceneManager instance;
    public static SceneManager GetInstance()
    {
        return instance;
    }

    public enum SceneType { MainMenu, Game }

    [Export] private PackedScene mainMenuScene;
    [Export] private PackedScene gameScene;
    [Export] private Label debug;

    private Node currentScene;
    private SceneType currentSceneType;
    private Timer transitionTimer;
    private SceneType sceneToLoad;
    private bool isTransitioning = false;
    private float loadTime = 0.3f;

    // ========================================================================
    // INIT
    // ========================================================================

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

        if (currentSceneType == SceneType.MainMenu)
            MenuManager.GetInstance().SetMenu(MenuManager.MenuState.MainMenu);
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    public override void _Process(double pDelta)
    {
        if (debug == null || !debug.Visible) return;

        string lText =
            $"Scene : {currentSceneType}\n" +
            $"FPS : {Engine.GetFramesPerSecond()}\n" +
            $"Time : {Engine.TimeScale:F2}s\n" +
            $"Freeze Time : {ScaleTime.currentTimeScale:F2}s\n";

        Player lPlayer = Player.GetInstance();
        if (lPlayer != null)
            lText += $"Player State : {lPlayer.currentState}\n";

        Hammer lHammer = Hammer.GetInstance();
        if (lHammer != null)
            lText += $"Hammer State : {lHammer.currentState}\n";

        debug.Text = lText;
    }

    // ========================================================================
    // INPUT
    // ========================================================================

    public override void _Input(InputEvent pInput)
    {
        if (debug == null) return;

        if (pInput.IsActionPressed(Property.INPUT_DEBUG))
            debug.Visible = !debug.Visible;
    }

    // ========================================================================
    // SCENE
    // ========================================================================

    public SceneType GetCurrentSceneType()
    {
        return currentSceneType;
    }

    private void SwitchScene(SceneType pSceneType)
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

        GameManager lGameManager = GameManager.GetInstance();
        if (lGameManager != null)
        {
            lGameManager.scrollSpeed = 0f;
            GameManager.scrollSpeedVelocity = Vector2.Zero;
        }

        Camera lCam = Camera.GetInstance();
        if (lCam != null)
        {
            Tween tween = CreateTween();
            tween.SetPauseMode(Tween.TweenPauseMode.Process);
            tween.TweenProperty(lCam, "global_position", Vector2.Zero, duration)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.InOut);
        }

        Player lPlayer = Player.GetInstance();
        if (lPlayer != null)
            lPlayer.AnimatedDespawn(duration);

        Timer timer = new Timer();
        timer.WaitTime = duration;
        timer.OneShot = true;
        AddChild(timer);

        Projectiles.ClearAll();
        Enemy.ClearAll(true, true);

        timer.Timeout += () =>
        {
            isTransitioning = false;
            timer.QueueFree();
            SwitchScene(pNextScene);
        };
        timer.Start();
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
        if (instance == this) instance = null;
        base.Dispose(pDisposing);
    }
}