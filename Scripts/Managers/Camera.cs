using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NToolsBox;
using Godot;

namespace Com.IsartDigital.Shmup.NManager;

public partial class Camera : Camera2D
{
    [ExportCategory("Scrolling Settings")]
    public float scrollSpeed = 300;
    [Export] public float accel = 3f;

    [ExportCategory("Vertical Follow Settings")]
    [Export] public float verticalFollowSpeed = 5f;
    [Export(PropertyHint.Range, "0, 1")] public float verticalLimitRatio = 0.98f;

    public bool isScrolling = false;

    private const float DEFAULT_OFFSET_MARGIN = 50f;
    private const float SPAWN_PADDING_Y = 40.0f;
    private const float HALF_DIVIDER = 2f;

    private static Camera instance;

    private bool fast;
    private float baseScrollSpeed;
    private float currentSpeed;
    private float targetSpeed;
    private Tween tween;
    private Vector2 startPosition;

    private float minYLimit;
    private float maxYLimit;

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

        AnchorMode = AnchorModeEnum.DragCenter;

        startPosition = GetViewportRect().Size / HALF_DIVIDER;
        Position = startPosition;

        CalculateVerticalLimits();

        baseScrollSpeed = scrollSpeed;
        currentSpeed = 0f;
        targetSpeed = scrollSpeed;
    }

    private void CalculateVerticalLimits()
    {
        float viewportHeight = GetViewportRect().Size.Y;

        float allowedMovementRange = (viewportHeight * (1f - verticalLimitRatio)) / 2f;

        minYLimit = startPosition.Y - allowedMovementRange;
        maxYLimit = startPosition.Y + allowedMovementRange;
    }

    public void ImFast()
    {
        fast = !fast;
        targetSpeed = fast ? baseScrollSpeed * 3f : baseScrollSpeed;
    }

    public override void _Process(double pDelta)
    {
        if (!isScrolling)
        {
            currentSpeed = 0f;
            return;
        }

        float lDelta = (float)pDelta;

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, accel * lDelta);
        float newX = Position.X + (currentSpeed * lDelta);

        float newY = Position.Y;
        Player player = Player.GetInstance();

        if (player != null && !player.isDeath)
        {
            newY = Mathf.Lerp(Position.Y, player.GlobalPosition.Y, verticalFollowSpeed * lDelta);
            newY = Mathf.Clamp(newY, minYLimit, maxYLimit);
        }
        else
        {
            newY = Mathf.Lerp(Position.Y, startPosition.Y, verticalFollowSpeed * lDelta);
        }

        Position = new Vector2(newX, newY);
    }

    public Vector2 GetRandomSpawnPositionRight(float pOffsetMargin = DEFAULT_OFFSET_MARGIN)
    {
        Rect2 lViewportRect = GetViewportRect();
        Vector2 lHalfSize = lViewportRect.Size / HALF_DIVIDER;

        float lSpawnX = GlobalPosition.X + lHalfSize.X + pOffsetMargin;
        float lMinY = (GlobalPosition.Y - lHalfSize.Y) + SPAWN_PADDING_Y;
        float lMaxY = (GlobalPosition.Y + lHalfSize.Y) - SPAWN_PADDING_Y;

        float lSpawnY = GameManager.rand.RandfRange(lMinY, lMaxY);

        return new Vector2(lSpawnX, lSpawnY);
    }

    public Vector2 GetScreenCenter()
    {
        return GlobalPosition;
    }

    public float GetTop()
    {
        return GlobalPosition.Y - (GetViewportRect().Size.Y / HALF_DIVIDER);
    }

    public float GetBottom()
    {
        return GlobalPosition.Y + (GetViewportRect().Size.Y / HALF_DIVIDER);
    }

    public Vector2 GetStartPosition()
    {
        return startPosition;
    }

    public void ResetPosition()
    {
        Position = startPosition;
    }

    protected override void Dispose(bool disposing)
    {
        if (tween != null && tween.IsValid()) tween.Kill();
        instance = null;
        base.Dispose(disposing);
    }
}