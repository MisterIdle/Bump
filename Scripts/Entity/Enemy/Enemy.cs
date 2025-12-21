using Godot;

public partial class Enemy : Entity
{
    protected float elapsedTime;

    [Export] protected float frequency = 1.0f;
    [Export] protected float amplitude = 1.0f;

    protected float angleX;
    protected float angleY;

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);
        elapsedTime += pDelta;

        angleX = Mathf.Sin(elapsedTime * frequency) * amplitude;
        angleY = Mathf.Cos(elapsedTime * frequency) * amplitude;
    }
}
