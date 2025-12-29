using Godot;

[Tool]
public partial class Obstacle : Area2D
{
    [Export] public Vector2 size = new Vector2(128, 128);
    [Export] public CollisionShape2D collision;
    [Export] public Node2D visual;
    [Export] public NinePatchRect patch;

    Vector2 lastSize;

    public override void _Ready()
    {
        lastSize = size;
        ApplySize();
    }

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint()) return;
        if (lastSize == size) return;

        lastSize = size;
        ApplySize();
    }

    void ApplySize()
    {
        if (!IsInsideTree()) return;
        if (collision == null || visual == null || patch == null) return;

        var rect = collision.Shape as RectangleShape2D;
        rect.Size = size;

        patch.Size = size;
        patch.Position = -size * 0.5f;
    }
}
