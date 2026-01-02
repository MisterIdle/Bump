using Godot;
using System.Collections.Generic; // Nécessaire pour les List<>

public partial class SceneManager : Node
{
    // ... (Le début du code reste inchangé : Singleton, Variables, _Ready, _Process) ...

    private static SceneManager instance;
    public static SceneManager GetInstance() { return instance; }

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

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else { QueueFree(); return; }

        transitionTimer = new Timer();
        transitionTimer.OneShot = true;
        transitionTimer.WaitTime = loadTime;
        transitionTimer.Timeout += OnTransitionFinished;
        AddChild(transitionTimer);

        SwitchScene(SceneType.MainMenu);
    }

    // ... (_Input et _Process inchangés) ...
    public override void _Input(InputEvent pInput)
    {
        if (pInput.IsActionPressed(Property.INPUT_DEBUG))
            debug.Visible = !debug.Visible;
    }

    public override void _Process(double pDelta)
    {
        if (!debug.Visible) return;
        // ... (Ton code de debug) ...
        debug.Text = "Debug info..."; // Simplifié pour la lecture
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

        // --- NETTOYAGE ---
        CleanupLevel();

        Camera cam = Camera.GetInstance();
        if (cam != null)
        {
            Tween tween = CreateTween();
            tween.SetPauseMode(Tween.TweenPauseMode.Process);
            tween.TweenProperty(cam, "global_position", Vector2.Zero, duration)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.InOut);
        }

        Player player = Player.GetInstance();
        if (player != null) player.AnimatedDespawn(duration);

        HUD hud = HUD.GetInstance();
        if (hud != null) hud.PlayExitAnimation(duration);

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

    // NOUVELLE VERSION SANS GROUPES
    private void CleanupLevel()
    {
        // 1. On récupère une position de référence (la caméra ou le centre)
        Vector2 centerPos = Vector2.Zero;
        if (Camera.GetInstance() != null) centerPos = Camera.GetInstance().GlobalPosition;

        // 2. Nettoyage des Projectiles via ta méthode statique
        // On récupère la liste
        List<Projectiles> projectiles = Projectiles.GetEnemiesProjectilesOnScreen(centerPos);
        if (projectiles != null)
        {
            // On boucle à l'envers par sécurité
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                if (IsInstanceValid(projectiles[i]))
                    projectiles[i].Destory(); // Utilise ta méthode Destory ou QueueFree
            }
        }

        List<Enemy> enemies = Enemy.GetEnemiesOnScreen(centerPos);
        if (enemies != null)
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (IsInstanceValid(enemies[i]))
                {
                    enemies[i].QueueFree();
                }
            }
        }
    }

    public void ReloadScene() { ChangeScene(SceneType.Game); }
    public void GoToMainMenu() { ChangeScene(SceneType.MainMenu); }

    private void OnTransitionFinished()
    {
        PackedScene lTargetScene = null;
        switch (sceneToLoad)
        {
            case SceneType.MainMenu: lTargetScene = mainMenuScene; break;
            case SceneType.Game: lTargetScene = gameScene; break;
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