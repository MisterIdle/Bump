using Godot;

using Com.IsartDigital.Shmup.NHUD;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NManager;

/// <summary>
/// A MODIF LE PARALLAX LAYER CAR FAUT MODIF LEURS MERDE
/// </summary>

/// <summary>
/// A MODIF LE PARALLAX LAYER CAR FAUT MODIF LEURS MERDE
/// </summary>
/// /// <summary>
/// A MODIF LE PARALLAX LAYER CAR FAUT MODIF LEURS MERDE
/// </summary>
/// /// <summary>
/// A MODIF LE PARALLAX LAYER CAR FAUT MODIF LEURS MERDE
/// </summary>
/// /// <summary>
/// A MODIF LE PARALLAX LAYER CAR FAUT MODIF LEURS MERDE
/// </summary>
/// /// <summary>
/// A MODIF LE PARALLAX LAYER CAR FAUT MODIF LEURS MERDE
/// </summary>
/// /// <summary>
/// A MODIF LE PARALLAX LAYER CAR FAUT MODIF LEURS MERDE
/// </summary>
/// /// <summary>
/// A MODIF LE PARALLAX LAYER CAR FAUT MODIF LEURS MERDE
/// </summary>
/// /// <summary>
/// A MODIF LE PARALLAX LAYER CAR FAUT MODIF LEURS MERDE
/// </summary>
/// /// <summary>
/// A MODIF LE PARALLAX LAYER CAR FAUT MODIF LEURS MERDE
/// </summary>
/// /// <summary>
/// A MODIF LE PARALLAX LAYER CAR FAUT MODIF LEURS MERDE
/// </summary>
/// /// <summary>
/// A MODIF LE PARALLAX LAYER CAR FAUT MODIF LEURS MERDE
/// </summary>
/// /// <summary>
/// A MODIF LE PARALLAX LAYER CAR FAUT MODIF LEURS MERDE
/// </summary>
/// /// <summary>
/// A MODIF LE PARALLAX LAYER CAR FAUT MODIF LEURS MERDE
/// </summary>
/// 
/// <summary>
/// A MODIF LE PARALLAX LAYER CAR FAUT MODIF LEURS MERDE
/// </summary>

public partial class SceneManager : Node
{
    private static SceneManager instance;
    public static SceneManager GetInstance()
    {
        return instance;
    }

    [Export] private PackedScene gameScene;
    [Export] private PackedScene debugScene;
    [Export] private Label debug;

    private Node currentScene;

    public enum SceneType { MainMenu, Game, Debug }
    private SceneType currentSceneType;
    private SceneType sceneToLoad;
    
    private bool isTransitioning = false;
    private float loadTime = 0.2f;

    public Timer transitionTimer;
    public Tween transitionTween;

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

        transitionTimer.Timeout += OnTransitionFinished;
        transitionTimer.OneShot = true;
        transitionTimer.WaitTime = 0.2f;

        AddChild(transitionTimer);

        if (currentSceneType == SceneType.MainMenu)
            MenuManager.GetInstance().SetMenu(MenuManager.MenuState.MainMenu);
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    public override void _Process(double pDelta)
    {
        if (!debug.Visible) return;

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
        {
            lText += $"Hammer State : {lHammer.currentState}\n";
            lText += $"Hammer Attack : {lHammer.attackForce}\n";
        }

        debug.Text = lText;
    }

    // ========================================================================
    // INPUT
    // ========================================================================

    public override void _Input(InputEvent pInput)
    {
        if (pInput.IsActionPressed(Property.INPUT_DEBUG))
            debug.Visible = !debug.Visible;

        if (pInput.IsActionPressed(Property.INPUT_DEBUG_SCENE))
        {
            SwitchScene(SceneType.Debug);
            MenuManager.GetInstance().SetMenu(MenuManager.MenuState.HUD);
        }
    }

    // ========================================================================
    // SCENE
    // ========================================================================

    public SceneType GetCurrentSceneType()
    {
        return currentSceneType;
    }

    public void ChangeGlobalScene(SceneType pNextScene)
    {
        if (isTransitioning) return;

        isTransitioning = true;
        float lDuration = 1.5f;

        GameManager lGameManager = GameManager.GetInstance();
        if (lGameManager != null)
        {
            lGameManager.scrollSpeed = 0f;
            GameManager.scrollSpeedVelocity = Vector2.Zero;
        }

        Player lPlayer = Player.GetInstance();
        if (lPlayer != null)
            lPlayer.AnimatedDespawn(lDuration);

        Camera lCam = Camera.GetInstance();
        if (lCam != null)
        {
            transitionTween?.Kill();
            transitionTween = CreateTween();

            transitionTween.SetPauseMode(Tween.TweenPauseMode.Process);
            transitionTween.TweenProperty(lCam, (string)Node2D.PropertyName.GlobalPosition, Vector2.Zero, lDuration)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.InOut);
        }

        SwitchScene(pNextScene);

        transitionTimer.Start();
    }

    private void SwitchScene(SceneType pSceneType)
    {
        sceneToLoad = pSceneType;

        if (currentScene != null)
        {
            Camera lCam = Camera.GetInstance();

            if (lCam != null)
                transitionTween.Finished += RemoveScene;
            else
                RemoveScene();
        }
    }

    private void RemoveScene()
    {
        RemoveChild(currentScene);
        currentScene.QueueFree();
        currentScene = null;
    }

    private void OnTransitionFinished()
    {
        PackedScene lTargetScene = null;

        switch (sceneToLoad)
        {
            case SceneType.Game:
                lTargetScene = gameScene;
                break;
            case SceneType.Debug:
                lTargetScene = debugScene;
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