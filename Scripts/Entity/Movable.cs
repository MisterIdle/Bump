using Godot;

public partial class Movable : GameObject
{
    [Export] public float speed;
    [Export] public bool affectedByScroll;
    [Export] private float lerpFactor = 0.05f;

    protected Vector2 currentVelocity;
    protected Vector2 velocity;

    protected override void Update(float pDelta)
    {
        pDelta = Time.GetDelta(pDelta);

        Vector2 targetVelocity = velocity * speed;

        if (affectedByScroll)
            targetVelocity += GameManager.scrollSpeedVelocity;

        currentVelocity = currentVelocity.Lerp(targetVelocity, lerpFactor);

        Position += currentVelocity * pDelta;
    }
}