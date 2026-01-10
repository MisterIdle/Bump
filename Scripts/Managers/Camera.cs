using Godot;

namespace Com.IsartDigital.Shmup.NManager;

public partial class Camera : Camera2D
{
    [Export] public float scrollSpeed = 100.0f;
    public bool isScrolling = false;

    private static Camera instance;
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
    }

    public override void _Process(double pDelta)
    {
        if (!isScrolling) return;

        Position += new Vector2(scrollSpeed * (float)pDelta, 0);
    }

    public Vector2 GetRandomSpawnPositionRight(float pOffsetMargin = 50f)
    {
        Rect2 lViewportRect = GetViewportRect();

        float lPaddingY = 40.0f;

        float lSpawnX = GlobalPosition.X + lViewportRect.Size.X + pOffsetMargin;

        float lMinY = GlobalPosition.Y + lPaddingY;
        float lMaxY = GlobalPosition.Y + lViewportRect.Size.Y - lPaddingY;

        float lSpawnY = GameManager.rand.RandfRange(lMinY, lMaxY);

        return new Vector2(lSpawnX, lSpawnY);
    }

    public Vector2 GetScreenCenter()
    {
        return GlobalPosition + (GetViewportRect().Size / 2f);
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