using Godot;
using System;

public partial class ShelledEnemy : Enemy
{
    [Export] protected NodePath shellPath;
    [Export] protected bool shellProtect;

    protected Shell shell;

    public override void Initiate()
    {
        base.Initiate();
        shell = GetNode<Shell>(shellPath);
    }

    protected override void TakeDamage()
    {
        if (shellProtect)
        {
            shell.ShellProtectAnimation();
            return;
        }

        base.TakeDamage();
    }
}
