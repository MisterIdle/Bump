using Godot;

public partial class Player : Entity
{
    [ExportCategory("Shot")]
    [Export] private float shotSpace;
    [Export] private PackedScene shotScene;

    private Shot lastShot;

    private static Player instance;
    public static Player GetInstance()
    {
        return instance;
    }

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else QueueFree();
    }

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

        Move(pDelta);

        if (CanFire())
            Fire();
    }

    private void Move(float pDelta)
    {
        Vector2 lDirection = Input.GetVector(
            Property.INPUT_LEFT, Property.INPUT_RIGHT, 
            Property.INPUT_UP, Property.INPUT_DOWN);

        velocity = lDirection;
    }

    private bool CanFire()
    {
        if (!Input.IsActionPressed(Property.INPUT_SHOT))
            return false;

        if(!IsInstanceValid(lastShot))
            return true;

        return lastShot.Position.X - Position.X > shotSpace;
    }

    private void Fire()
    {
        lastShot = Shot.Create(shotScene, GlobalPosition, Vector2.Right);
        GameManager.GetInstance().gameContainer.AddChild(lastShot);
    }
}