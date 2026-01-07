using Godot;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable;

public partial class Movable : GameObject
{
    [Export] protected int score = 1000;

    [ExportCategory("Speed")]
    [Export] public float speed;
    [Export] private float lerpFactor = 0.05f;

    [ExportCategory("Oscillation")]
    [Export] protected float frequency = 1.0f;
    [Export] protected float amplitude = 1.0f;

    protected float elapsedTime;
    protected float angleX;
    protected float angleY;

    protected Vector2 currentVelocity;
    protected Vector2 velocity;

    public enum MoveDirection { None, Left, Right }
    [Export] public MoveDirection direction;

    public override void Initiate()
    {
        base.Initiate();
        velocity = GetDirectionVector();
    }

    protected override void Update(float pDelta)
    {
        pDelta = ScaleTime.GetDelta(pDelta);
        elapsedTime += pDelta;

        Vector2 lTargetVelocity = velocity * speed;

        angleX = Mathf.Sin(elapsedTime * frequency) * amplitude;
        angleY = Mathf.Cos(elapsedTime * frequency) * amplitude;

        currentVelocity = currentVelocity.Lerp(lTargetVelocity, lerpFactor);
        Position += currentVelocity * pDelta;

        if (Utils.OutOfBoundAxes(GlobalPosition, 100f).X == -1)
        {
            if (this is not Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer.Player)
                QueueFree();
        }
    }

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
            default:
                velocity = Vector2.Zero;
                break;
        }

        return velocity;
    }
}