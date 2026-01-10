using Godot;

using Com.IsartDigital.Shmup.NGameObject;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NCollectibles;

namespace Com.IsartDigital.Shmup.NManager;

public partial class GameManager : Node2D
{
    [ExportCategory("Base enable")]
    [Export] public GameObject[] forceEnable;

    [ExportCategory("Collectibles")]
    [Export] public PackedScene healthScene;
    [Export] public int healthPercent = 25;

    [ExportCategory("Self")]
    [Export] public Node2D gameContainer;
    [Export] public Node2D projectilesContainer;
    [Export] public Node2D enemyContainer;

    public static RandomNumberGenerator rand = new RandomNumberGenerator();

    private const int CHANCE_MIN = 0;
    private const int CHANCE_MAX = 100;
    private const int LOOP_MIN = 0;

    private static GameManager instance;
    public GameManager() : base() { }

    public static GameManager GetInstance()
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

        for (int i = forceEnable.Length - 1; i >= LOOP_MIN; i--)
            if (!forceEnable[i].enable) forceEnable[i].Initiate();
    }

    // ========================================================================
    // COLLECTIBLE
    // ========================================================================

    public static void CreateHealthCollectible(Vector2 pPos)
    {
        GetInstance().CallDeferred(MethodName.CallCreateHealthCollectible, pPos);
    }

    private void CallCreateHealthCollectible(Vector2 pPos)
    {
        int lChance = rand.RandiRange(CHANCE_MIN, CHANCE_MAX);

        if (lChance < healthPercent)
        {
            HeartCollectible.Create(healthScene, pPos);
        }
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}
