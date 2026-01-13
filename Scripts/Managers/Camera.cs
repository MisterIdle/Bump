using Com.IsartDigital.Shmup.NToolsBox;
using Godot;

namespace Com.IsartDigital.Shmup.NManager;

public partial class Camera : Camera2D
{
    [Export] public float scrollSpeed = 200;
    public bool isScrolling = false;

    private const float DEFAULT_OFFSET_MARGIN = 50f;
    private const float SPAWN_PADDING_Y = 40.0f;
    private const float HALF_DIVIDER = 2f;

    private static Camera instance;

    private bool fast;
    private float baseScrollSpeed;

    public static Camera GetInstance()
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

        baseScrollSpeed = scrollSpeed;
    }

    public void ImFast()
    {
        fast = !fast;
        scrollSpeed = fast ? baseScrollSpeed * 3f : baseScrollSpeed;
    }

    public override void _Process(double pDelta)
    {
        if (!isScrolling) return;

        float lDelta = (float)pDelta;

        Position += Vector2.Right * (scrollSpeed * ScaleTime.GetDelta(lDelta));
    }

    public Vector2 GetRandomSpawnPositionRight(float pOffsetMargin = DEFAULT_OFFSET_MARGIN)
    {
        Rect2 lViewportRect = GetViewportRect();

        float lSpawnX = GlobalPosition.X + lViewportRect.Size.X + pOffsetMargin;

        float lMinY = GlobalPosition.Y + SPAWN_PADDING_Y;
        float lMaxY = GlobalPosition.Y + lViewportRect.Size.Y - SPAWN_PADDING_Y;

        float lSpawnY = GameManager.rand.RandfRange(lMinY, lMaxY);

        return new Vector2(lSpawnX, lSpawnY);
    }

    public Vector2 GetScreenCenter()
    {
        return GlobalPosition + (GetViewportRect().Size / HALF_DIVIDER);
    }

    public float GetTop()
    {
        return GlobalPosition.Y;
    }

    public float GetBottom()
    {
        return GlobalPosition.Y + GetViewportRect().Size.Y;
    }

    public void ResetPosition()
    {
        Position = Vector2.Zero;
    }

    protected override void Dispose(bool disposing)
    {
        instance = null;
        base.Dispose(disposing);
    }
}
