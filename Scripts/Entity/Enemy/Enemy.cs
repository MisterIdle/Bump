using Godot;

public partial class Enemy : Entity
{
    [Export] protected NodePath shellPath;
    protected Shell shell;

    public override void _Ready()
    {
        // Obligatoire sinon shell généraliste, étrange
        shell = GetNode<Shell>(shellPath);
    }
}
