using Godot;

public partial class Enemy : Entity
{
    protected float elapsedTime;

    [Export] protected float frequency = 1.0f;
    [Export] protected float amplitude = 1.0f;

    [Export] protected bool oneShotByHammer;

    protected float angleX;
    protected float angleY;

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        elapsedTime += pDelta;

        angleX = Mathf.Sin(elapsedTime * frequency) * amplitude;
        angleY = Mathf.Cos(elapsedTime * frequency) * amplitude;
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        if (pArea.Owner is Hammer && oneShotByHammer)
            health = 0;

       EntityHit();
    }
}
