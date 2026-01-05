using Godot;

namespace Com.IsartDigital.Shmup.NGameObject;

[Tool]
public partial class Obstacle : Area2D
{
    [Export] public Vector2 size = new Vector2(128, 128);
    [Export] public CollisionShape2D collision;
    [Export] public Node2D visual;
    [Export] public NinePatchRect patch;

    Vector2 lastSize;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        lastSize = size;
        ApplySize();
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    public override void _Process(double delta)
    {
        if (lastSize == size) return;

        lastSize = size;
        ApplySize();
    }

    void ApplySize()
    {
        RectangleShape2D lRect = (RectangleShape2D)collision.Shape;
        lRect.Size = size;

        patch.Size = size;
        patch.Position = -size * 0.5f;
    }
}
