using Godot;

public partial class GameManager : Node2D
{
    [ExportCategory("Scroll")]
    [Export] public float scrollSpeed = 250f;
    public static Vector2 scrollSpeedVelocity;

    [ExportCategory("Base enable")]
    [Export] public GameObject[] forceEnable;

    [ExportCategory("Collectibles")]
    [Export] public PackedScene healthScene;
    [Export] public int healthPercent = 20;

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

    public static void CreateHealthCollectible(Vector2 pPos)
    {
        GameManager lSelf = GetInstance();
        lSelf.CallDeferred(nameof(CallCreateHealthCollectible), pPos);
    }

    private void CallCreateHealthCollectible(Vector2 pPos)
    {
        int lChance = rand.RandiRange(0, 100);

        if (lChance < healthPercent)
            HeartCollectible.Create(healthScene, pPos);
    }

    public void AddCoin()
    {
        coin++;
        GD.Print("COIN:" + coin);
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}
