using Godot;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable;

public partial class Movable : GameObject
{
    [Export] protected int score = 1000;

    public enum MoveDirection { None, Left, Right }

    [ExportCategory("Direction")]
    [Export] public MoveDirection direction;

    [ExportCategory("Speed")]
    [Export] public float speed;

    [Export] public bool affectedByScroll = true;

    [Export] private float lerpFactor = 0.05f;

    [ExportCategory("Oscillation")]
    [Export] protected float frequency = 1.0f;
    [Export] protected float amplitude = 1.0f;

    protected float elapsedTime;
    protected float angleX;
    protected float angleY;

    protected Vector2 currentVelocity;
    protected Vector2 velocity;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void Initiate()
    {
        base.Initiate();
        velocity = GetDirectionVector();
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    protected override void Update(float pDelta)
    {
        float lGameDelta = ScaleTime.GetDelta(pDelta);
        elapsedTime += lGameDelta;

        Vector2 lTargetVelocity = velocity * speed;

        angleX = Mathf.Sin(elapsedTime * frequency) * amplitude;
        angleY = Mathf.Cos(elapsedTime * frequency) * amplitude;

        currentVelocity = currentVelocity.Lerp(lTargetVelocity, lerpFactor);

        Vector2 lMotion = currentVelocity * lGameDelta;

        if (affectedByScroll)
            lMotion.X += Camera.GetInstance().scrollSpeed * lGameDelta;

        GlobalPosition += lMotion;
    }

    // ========================================================================
    // UTILS
    // ========================================================================

    private Vector2 GetDirectionVector()
    {
        switch (direction)
        {
            case MoveDirection.Left:
                velocity = Vector2.Left;
                break;
            case MoveDirection.Right:
                velocity = Vector2.Right;
                break;
            case MoveDirection.None:
                velocity = Vector2.Zero;
                break;
            default:
                break;
        }

        return velocity;
    }
}