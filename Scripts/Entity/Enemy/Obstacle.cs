using Godot;

[Tool]
public partial class Obstacle : Area2D
{
    [Export]
    public Vector2 Size = new Vector2(128, 128);

    [Export]
    public CollisionShape2D Collision;

    [Export]
    public Node2D Visual;

    [Export]
    public NinePatchRect Patch;

    Vector2 _lastSize;

    public override void _Ready()
    {
        _lastSize = Size;
        ApplySize();
    }

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint()) return;
        if (_lastSize == Size) return;

        _lastSize = Size;
        ApplySize();
    }

    void ApplySize()
    {
        if (!IsInsideTree()) return;
        if (Collision == null || Visual == null || Patch == null) return;

        var rect = Collision.Shape as RectangleShape2D;
        rect.Size = Size;

        Patch.Size = Size;
        Patch.Position = -Size * 0.5f;
    }
}
