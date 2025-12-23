using Godot;

public partial class Shot : Projectiles
{
    [Export] private bool rotation;

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

        if (rotation)
            RotationDegrees += rotationSpeed * pDelta;
    }

    public static Shot Create(PackedScene pScene, Vector2 pPos, Vector2 pWay, float pAngle = 0)
    {
        Shot lShot = (Shot)pScene.Instantiate();
        lShot.Position = pPos;
        lShot.SetInitialMovement(pAngle, pWay);
        lShot.Initiate();
        return lShot;
    }

    public void SetVisualParameters(Color color, float scale)
    {
        Modulate = color;
        Scale = Vector2.One * scale;
    }

    public void SetSpeed(float pSpeed)
    {
        speed = pSpeed;
    }

    private void SetInitialMovement(float pAngle, Vector2 pWay)
    {
        float lSpreadAngle = GameManager.rand.RandfRange(-pAngle, pAngle);
        float lFinalAngle = pWay.Angle() + lSpreadAngle;
        velocity = Vector2.FromAngle(lFinalAngle) * pWay.Length();
        Rotation = lFinalAngle;
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);
        QueueFree();
        Particule.Create(popScene, Position);
    }
}
