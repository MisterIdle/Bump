using Godot;

public partial class ShelledEnemy : Enemy
{
    [Export] public bool shellProjectileProtect;

    protected Shell shell;
    protected Wing wing;

    [Export] private NodePath shellPath;
    [Export] private NodePath wingPath;

    protected Vector2 playerDirection;
    protected Vector2 playerPos;

    [Export] protected bool facePlayer;
    protected bool faceLeft;

    public override void Initiate()
    {
        base.Initiate();

        shell = (Shell)GetNode(shellPath);
        wing = (Wing)GetNode(wingPath);
    }

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        playerPos = Player.GetInstance().GlobalPosition;
        playerDirection = Utils.DirectionTo(GlobalPosition, playerPos);

        if (facePlayer)
        {
            bool lFaceLeft = playerDirection.X < 0;

            if (lFaceLeft != faceLeft)
            {
                faceLeft = lFaceLeft;
                Flip(faceLeft);
            }
        }
    }

    public override void EntityHit(int pDamage) 
    {
        base.EntityHit(pDamage);

        if (health <= 0)
        {
            shell.ShellDestructionAnimation();
            wing.SetSpeedDouble(true);
        }
    }

    protected override void OnCollide(Area2D pArea)
    {
        if (pArea is Projectiles && shellProjectileProtect)
        {
            shell.ShellProtectAnimation();
            return;
        }

        if (pArea is Player lPlayer && lPlayer.invincible)
        {
            shell.ShellProtectAnimation();
            return;
        }

        base.OnCollide(pArea);
        shell.ShellDamageAnimation();
    }
}
