using Godot;

public partial class Player : Entity
{
    [Export] private Marker2D spawnPoint;

    [ExportCategory("Shot")]
    [Export] private float shotSpace;
    [Export] private PackedScene shotScene;

    [Export] private Hammer hammer;

    private Shot lastShot;

    private bool isDashing;
    private float dashSpeed;

    private Timer timerDash;
    private Timer respawnTimer;

    private static Player instance;
    public static Player GetInstance()
    {
        return instance;
    }

    public override void Initiate()
    {
        base.Initiate();
        hammer.Initiate();

        timerDash = new Timer();
        timerDash.OneShot = true;
        timerDash.Timeout += DashFinish;
        AddChild(timerDash);
    }

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else QueueFree();
    }

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        Move(pDelta);

        hammer.Attack(pDelta);

        if (CanFire())
            Fire();
    }

    private void Move(float pDelta)
    {
        Vector2 lDirection = Vector2.Zero;

        if (isDashing)
            lDirection = Vector2.Right * dashSpeed * pDelta;
        else
            lDirection = Input.GetVector(Property.INPUT_LEFT, Property.INPUT_RIGHT, Property.INPUT_UP, Property.INPUT_DOWN);

        velocity = lDirection;
    }

    public void Dash(float pDuration)
    {
        if (isDashing)
            return;

        isDashing = true;
        dashSpeed = speed * 1.5f;
        timerDash.Start(pDuration);
    }

    private void DashFinish()
    {
        isDashing = false;
    }

    private bool CanFire()
    {
        if (!Input.IsActionPressed(Property.INPUT_SHOT))
            return false;

        if (!IsInstanceValid(lastShot))
            return true;

        return lastShot.Position.X - Position.X > shotSpace;
    }

    private void Fire()
    {
        lastShot = Shot.Create(shotScene, GlobalPosition, Vector2.Right);
        GameManager.GetInstance().gameContainer.AddChild(lastShot);
        Bump();
    }

    public override void EntityHit()
    {
        base.EntityHit();

        if (isDeath) return;

        GlobalPosition = spawnPoint.GlobalPosition;
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        EntityHit();
    }
}
