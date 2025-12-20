using Godot;

public partial class GameManager : Node
{
    [Export] public float scrollSpeed = 100f;
    public static Vector2 scrollSpeedVelocity;

    [Export] public GameObject[] forceEnable;

    [Export] public Node2D gameContainer;

    public RandomNumberGenerator rand = new RandomNumberGenerator();

    private static GameManager instance;

    public GameManager() : base() { }

    public static GameManager GetInstance()
    {
        return instance;
    }

    public override void _Ready()
    {
        if (instance == null)
            instance = this;
        else QueueFree();

        for (int i = forceEnable.Length - 1; i >= 0; i--)
            forceEnable[i].Initiate();
    }

    public override void _Process(double delta)
    {
        scrollSpeedVelocity = Vector2.Right * scrollSpeed;
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}
