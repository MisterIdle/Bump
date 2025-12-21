using Godot;

public partial class ShelledEnemy : Enemy
{
    [Export] public bool shellProjectileProtect;

    protected Shell shell;
    [Export] private NodePath shellPath;

    public override void Initiate()
    {
        base.Initiate();

        shell = (Shell)GetNode(shellPath);
        shell.ownerEnemy = this;
    }

    public override void EntityHit() 
    {
        base.EntityHit();

        if (health <= 0)
            shell.ShellDestructionAnimation();
    }

    protected override void OnCollide(Area2D pArea)
    {
        if (pArea is Projectiles && shellProjectileProtect)
        {
            shell.ShellProtectAnimation();
            return;
        }

        base.OnCollide(pArea);
        shell.ShellDamageAnimation();
    }
}
