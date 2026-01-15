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

    public override void Initiate()
    {
        base.Initiate();
        velocity = GetDirectionVector();
    }

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

    private Vector2 GetDirectionVector()
    {
        switch (direction)
        {
            case MoveDirection.Left:
                return Vector2.Left.Rotated(Rotation);
            case MoveDirection.Right:
                return Vector2.Right.Rotated(Rotation);
            case MoveDirection.None:
                return Vector2.Zero;
            default:
                return Vector2.Zero;
        }
    }
}