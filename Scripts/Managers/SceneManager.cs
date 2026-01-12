using Godot;
using System;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NHUD;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NManager;

public partial class SceneManager : Node
{
    private static SceneManager instance;
    public static SceneManager GetInstance() => instance;

    [ExportCategory("Scenes")]
    [Export] private PackedScene gameScene;
    [Export] private PackedScene debugScene;

    [ExportCategory("References")]
    [Export] private ParallaxLayer gameLayer;
    [Export] private Label debugLabel;

    public enum SceneType { MainMenu, Game, Debug }
    private SceneType currentSceneType;
    private SceneType sceneToLoad;
    private Node currentScene;

    private bool isTransitioning = false;
    private float loadDelay = 0.4f;
    private double sceneDuration = 0;

    private Timer transitionTimer;
    private Tween transitionTween;

    // ========================================================================
    // INIT & LIFECYCLE
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
        transitionTimer.WaitTime = loadDelay;

        AddChild(transitionTimer);
    }

    // ========================================================================
    // UPDATE LOOP
    // ========================================================================

    public override void _Process(double delta)
    {
        if (!isTransitioning)
            sceneDuration += delta;

        UpdateDebugDisplay();
    }

    public override void _Input(InputEvent pInt)
    {
        if (pInt.IsActionPressed(Property.INPUT_DEBUG))
            debugLabel.Visible = !debugLabel.Visible;

        if (pInt.IsActionPressed(Property.INPUT_DEBUG_SCENE))
        {
            AudioManager.GetInstance().Play(AudioManager.AudioType.UIStart);
            MenuManager.GetInstance().SetMenu(MenuManager.MenuState.None);

            HUD.GetInstance()?.ResetHUD();
            ChangeGlobalScene(SceneType.Debug);
        }
    }

    // ========================================================================
    // SCENE MANAGEMENT
    // ========================================================================

    public void ChangeGlobalScene(SceneType nextScene)
    {
        if (isTransitioning) return;

        isTransitioning = true;
        float duration = 0.5f;

        Player player = Player.GetInstance();
        player?.AnimatedDespawn(duration);

        Camera cam = Camera.GetInstance();
        if (cam != null)
        {
            cam.isScrolling = false;

            transitionTween?.Kill();
            transitionTween = CreateTween();
            transitionTween.SetPauseMode(Tween.TweenPauseMode.Process);

            transitionTween.TweenProperty(cam, Node2D.PropertyName.GlobalPosition.ToString(), Vector2.Zero, duration)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.InOut);

            transitionTween.Finished += () => LoadSceneProcess(nextScene);
        }
        else
        {
            LoadSceneProcess(nextScene);
        }
    }

    private void LoadSceneProcess(SceneType sceneType)
    {
        sceneToLoad = sceneType;
        RemoveCurrentScene();
        transitionTimer.Start();
    }

    private void RemoveCurrentScene()
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
        PackedScene targetScene = sceneToLoad switch
        {
            SceneType.Game => gameScene,
            SceneType.Debug => debugScene,
            _ => null
        };

        if (targetScene != null)
        {
            currentScene = targetScene.Instantiate();
            gameLayer.AddChild(currentScene);
        }

        currentSceneType = sceneToLoad;

        isTransitioning = false;
        sceneDuration = 0;

        if (currentSceneType != SceneType.MainMenu)
        {
            Camera cam = Camera.GetInstance();
            if (cam != null)
            {
                cam.ResetPosition();
                cam.isScrolling = true;
            }
        }
    }

    // ========================================================================
    // DEBUG DISPLAY
    // ========================================================================

    private void UpdateDebugDisplay()
    {
        if (!debugLabel.Visible) return;

        TimeSpan t = TimeSpan.FromSeconds(sceneDuration);
        string formattedTime = $"{t.Minutes:D2}:{t.Seconds:D2}";

        string text =
            $"--- [ SYSTEM ] ---\n" +
            $"Duration : {formattedTime}\n" +
            $"Scene    : {currentSceneType}\n" +
            $"FPS      : {Engine.GetFramesPerSecond()}\n" +
            $"TimeScale: {Engine.TimeScale:F2}\n";

        Player player = Player.GetInstance();
        if (player != null)
        {
            text += $"\n--- [ PLAYER ] ---\n" +
                    $"State : {player.currentState}\n";
        }

        Hammer hammer = Hammer.GetInstance();
        if (hammer != null)
        {
            text += $"Hammer ATK   : {hammer.attackForce}\n" +
                    $"Hammer State : {hammer.currentState}\n";
        }

        debugLabel.Text = text;
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}