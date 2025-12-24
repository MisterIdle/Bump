using Godot;

public partial class GameManager : Node2D
{
    [ExportCategory("Scoll")]
    [Export] public float scrollSpeed = 100f;
    public static Vector2 scrollSpeedVelocity;

    [ExportCategory("Base enable")]
    [Export] public GameObject[] forceEnable;

    [ExportCategory("Collectibles")]
    [Export] public PackedScene[] collectibleScenes;

    [ExportCategory("Self")]
    [Export] public Node2D gameContainer;

    [ExportCategory("Global Score")]
    [Export] public int coin;
    [Export] public int score;

    public Vector2 screenSize;
    public Vector2 screenSizeCenterX;

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
        Vector2 lViewportSize = GetViewportRect().Size;
        Vector2 lCameraPos = Camera.GetInstance().GlobalPosition;

        screenSize = lViewportSize + lCameraPos;
        screenSizeCenterX = new Vector2(lCameraPos.X + (lViewportSize.X / 2), 0);

        scrollSpeedVelocity = Vector2.Right * scrollSpeed;
    }

    public static void CreateRandomCollectibles(Vector2 pPos)
    {
        GameManager lSelf = GetInstance();
        int lRandomIndex = rand.RandiRange(0, lSelf.collectibleScenes.Length - 1);
        Collectibles.Create(lSelf.collectibleScenes[lRandomIndex], pPos);
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
