using Godot;

public partial class GameManager : Node2D
{
    [ExportCategory("Scoll")]
    [Export] public float scrollSpeed = 100f;
    public static Vector2 scrollSpeedVelocity;

    [ExportCategory("Base enable")]
    [Export] public GameObject[] forceEnable;

    [ExportCategory("Self")]
    [Export] public Node2D gameContainer;

    [ExportCategory("Global Score")]
    [Export] public int coin;
    [Export] public int score;

    public Vector2 screenSize;

    public static RandomNumberGenerator rand = new RandomNumberGenerator();

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
            if (!forceEnable[i].enable) forceEnable[i].Initiate();
    }

    public override void _Process(double pDelta)
    {
        screenSize = GetViewportRect().Size + CameraManager.GetInstance().GlobalPosition;

        scrollSpeedVelocity = Vector2.Right * scrollSpeed;
    }

    public void AddCoin()
    {
        coin++;
        GD.Print("COIN:" + coin);
    }

    public void UpdateScore(int pScore) 
    { 
        score += pScore;
        GD.Print("SCORE:" + score);
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}
