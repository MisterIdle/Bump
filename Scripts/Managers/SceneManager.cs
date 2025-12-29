using Godot;
using System;

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

    private Node currentScene;

    public override void _Ready()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            QueueFree();
            return;
        }

        SwitchScene(SceneType.MainMenu);
    }

    public void SwitchScene(SceneType pSceneType)
    {
        PackedScene targetScene;

        switch (pSceneType)
        {
            case SceneType.MainMenu:
                targetScene = mainMenuScene;
                break;
            case SceneType.Game:
                targetScene = gameScene;
                break;
            default:
                GD.PrintErr("SceneType inconnu !");
                return;
        }

        if (currentScene != null)
        {
            RemoveChild(currentScene);
            currentScene.QueueFree();
        }

        currentScene = targetScene.Instantiate();
        AddChild(currentScene);
    }
}
