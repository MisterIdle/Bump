using Godot;
using Com.IsartDigital.Shmup.NGameObject;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NCollectibles;

namespace Com.IsartDigital.Shmup.NManager;

public partial class GameManager : Node2D
{
    [ExportCategory("Scroll")]
    [Export] public float scrollSpeed = 150f;

    [ExportCategory("Base enable")]
    [Export] public GameObject[] forceEnable;

    [ExportCategory("Collectibles")]
    [Export] public PackedScene healthScene;
    [Export] public int healthChance = 25;

    [ExportCategory("Container")]
    [Export] public Node2D gameContainer;
    [Export] public Node2D projectilesContainer;
    [Export] public Node2D enemyContainer;

    public static RandomNumberGenerator rand = new RandomNumberGenerator();

    private static GameManager instance;

    public GameManager() : base() { }

    public static GameManager GetInstance()
    {
        return instance;
    }

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else
        {
            QueueFree();
            return;
        }

        if (forceEnable != null)
        {
            for (int i = forceEnable.Length - 1; i >= 0; i--)
                if (!forceEnable[i].enable) forceEnable[i].Initiate();
        }
    }

    public override void _Process(double pDelta)
    {
        Camera lCam = Camera.GetInstance();
        if (lCam != null)
        {
            lCam.GlobalPosition += Vector2.Right * scrollSpeed * (float)pDelta;
        }
    }

    public static void CreateHealthCollectible(Vector2 pPos)
    {
        GameManager lGameManager = GetInstance();
        lGameManager.CallDeferred(nameof(CallCreateHealthCollectible), pPos);
    }

    private void CallCreateHealthCollectible(Vector2 pPos)
    {
        int lChance = rand.RandiRange(0, 100);

        if (lChance < healthChance)
            HeartCollectible.Create(healthScene, pPos);
    }

    protected override void Dispose(bool pDisposing)
    {
        if (instance == this) instance = null;
        base.Dispose(pDisposing);
    }
}