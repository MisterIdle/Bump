using Com.IsartDigital.Shmup.NGameObject;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NCollectibles;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;
using Godot;

namespace Com.IsartDigital.Shmup.NManager;

public partial class GameManager : Node2D
{
    [ExportCategory("Base enable")]
    [Export] public GameObject[] forceEnable;

    [ExportCategory("Collectibles")]
    [Export] public PackedScene healthScene;
    [Export] public PackedScene powScene;
    [Export] public PackedScene flowerScene;

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

    public void SpawnCollectible(Enemy.ItemType pType, Vector2 pPos)
    {
        CallDeferred(MethodName.DeferredSpawnCollectible, (int)pType, pPos);
    }

    private void DeferredSpawnCollectible(int pTypeInt, Vector2 pPos)
    {
        Enemy.ItemType pType = (Enemy.ItemType)pTypeInt;

        switch (pType)
        {
            case Enemy.ItemType.Health:
                HeartCollectible.Create(healthScene, pPos);
                break;

            case Enemy.ItemType.Flower:
                FlowerCollectible.Create(flowerScene, pPos);
                break;

            case Enemy.ItemType.Pow:
                PowCollectible.Create(powScene, pPos);
                break;

            case Enemy.ItemType.None:
            default:
                break;
        }
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}
