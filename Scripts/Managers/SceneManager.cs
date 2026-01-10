using Godot;

using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NHUD;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NManager;

public partial class SceneManager : Node
{
    [Export] private PackedScene gameScene;
    [Export] private PackedScene debugScene;

    [Export] private ParallaxLayer gameLayer;

    [Export] private Label debug;

    private Node currentScene;

    public enum SceneType { MainMenu, Game, Debug }
    private SceneType currentSceneType;
    private SceneType sceneToLoad;

    private bool isTransitioning = false;
    private float loadTime = 0.4f;

    public Timer transitionTimer;
    public Tween transitionTween;

    private static SceneManager instance;
    public static SceneManager GetInstance()
    {
        return instance;
    }

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
        transitionTimer.WaitTime = loadTime;

        AddChild(transitionTimer);
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    public override void _Process(double pDelta)
    {
        Debug();
    }

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
    // SCENES
    // ========================================================================

    public void ChangeGlobalScene(SceneType pNextScene)
    {
        if (isTransitioning) return;

        isTransitioning = true;
        float lDuration = 0.5f;

        Player lPlayer = Player.GetInstance();
        if (lPlayer != null)
            lPlayer.AnimatedDespawn(lDuration);

        Camera lCam = Camera.GetInstance();
        if (lCam != null)
        {
            lCam.isScrolling = false;

            transitionTween?.Kill();
            transitionTween = CreateTween();

            transitionTween.SetPauseMode(Tween.TweenPauseMode.Process);
            transitionTween.TweenProperty(lCam, (string)Node2D.PropertyName.GlobalPosition, Vector2.Zero, lDuration)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.InOut);
        }

        SwitchScene(pNextScene);
    }

    private void SwitchScene(SceneType pSceneType)
    {
        sceneToLoad = pSceneType;

        transitionTimer.Start();

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
        if (currentScene != null)
        {
            gameLayer.RemoveChild(currentScene);
            currentScene.QueueFree();
            currentScene = null;
        }
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
            gameLayer.AddChild(currentScene);
        }

        currentSceneType = sceneToLoad;

        isTransitioning = false;

        if (currentSceneType != SceneType.MainMenu)
        {
            Camera lCam = Camera.GetInstance();
            if (lCam != null)
            {
                lCam.ResetPosition();
                lCam.isScrolling = true;
            }
        }
    }

    // ========================================================================
    // DEBUG
    // ========================================================================

    private void Debug()
    {
        if (!debug.Visible) return;

        string lText =
            $"\n--- [ SYSTEM ] ---\n" +
            $"Scene : {currentSceneType}\n" +
            $"FPS : {Engine.GetFramesPerSecond()}\n" +
            $"Time : {Engine.TimeScale:F2}s\n" +
            $"Freeze Time : {ScaleTime.currentTimeScale:F2}s\n";

        Player lPlayer = Player.GetInstance();
        if (lPlayer != null)
        {
            lText += $"\n--- [ PLAYER ] ---\n" +
                $"State : {lPlayer.currentState}\n";
        }

        Hammer lHammer = Hammer.GetInstance();
        if (lHammer != null)
        {
            lText += $"Hammer ATK : {lHammer.attackForce}\n" +
                $"Hammer State : {lHammer.currentState}\n" +
                $"Hammer Attack : {lHammer.attackForce}\n";
        }

        debug.Text = lText;
    }

    protected override void Dispose(bool pDisposing)
    {
        if (instance == this) instance = null;
        base.Dispose(pDisposing);
    }
}