using Godot;
using System;

public partial class GameObject : Area2D
{
    public bool enable = false;

    [ExportCategory("Visual")]
    [Export] public Sprite2D sprite;
    public Vector2 baseScale;
    public Vector2 baseSpriteScale;

    public virtual void Initiate()
    {
        AreaEntered += OnCollide;

        baseScale = Scale;

        if (sprite != null)
            baseSpriteScale = sprite.Scale;

        GD.Print(Name + " base: " + baseSpriteScale);

        enable = true;

        GD.Print(Name + " enable");
    }

    public override void _Process(double pDelta)
    {
        if (!enable) return;

        float lDelta = (float)pDelta;

        Update(lDelta);
    }

    protected virtual void Update(float pDelta) { }
    protected virtual void OnCollide(Area2D pArea) { }
}
