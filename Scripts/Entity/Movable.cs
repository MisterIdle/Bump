using Godot;

public partial class Movable : GameObject
{
    [Export] protected int score = 1000;

    [ExportCategory("Speed")]
    [Export] public float speed;
    [Export] public bool affectedByScroll;
    [Export] private float lerpFactor = 0.05f;

    [ExportCategory("Oscillation")]
    [Export] protected float frequency = 1.0f;
    [Export] protected float amplitude = 1.0f;

    protected float elapsedTime;
    protected float angleX;
    protected float angleY;

    protected Vector2 currentVelocity;
    protected Vector2 velocity;

    protected override void Update(float pDelta)
    {
        pDelta = Time.GetDelta(pDelta);
        elapsedTime += pDelta;

        Vector2 targetVelocity = velocity * speed;

        angleX = Mathf.Sin(elapsedTime * frequency) * amplitude;
        angleY = Mathf.Cos(elapsedTime * frequency) * amplitude;

        if (affectedByScroll)
            targetVelocity += GameManager.scrollSpeedVelocity;

        currentVelocity = currentVelocity.Lerp(targetVelocity, lerpFactor);
        Position += currentVelocity * pDelta;
    }
}
