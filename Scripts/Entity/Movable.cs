using Godot;

using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable;

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

    // ========================================================================
    // UPDATE
    // ========================================================================

    protected override void Update(float pDelta)
    {
        pDelta = ScaleTime.GetDelta(pDelta);
        elapsedTime += pDelta;

        Vector2 lTargetVelocity = velocity * speed;

        angleX = Mathf.Sin(elapsedTime * frequency) * amplitude;
        angleY = Mathf.Cos(elapsedTime * frequency) * amplitude;

        if (affectedByScroll)
            lTargetVelocity += GameManager.scrollSpeedVelocity;

        currentVelocity = currentVelocity.Lerp(lTargetVelocity, lerpFactor);
        Position += currentVelocity * pDelta;
    }
}