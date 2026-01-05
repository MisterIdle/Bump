using Godot;
using System;

namespace Com.IsartDigital.Shmup.NGameObject;

public partial class GameObject : Area2D
{
    public bool enable = false;

    [ExportCategory("Visual")]
    [Export] public Sprite2D sprite;
    public Vector2 baseScale;
    public Vector2 baseSpriteScale;

    // ========================================================================
    // INIT
    // ========================================================================

    public virtual void Initiate()
    {
        AreaEntered += OnCollide;

        baseScale = Scale;

        if (sprite != null && IsInstanceValid(sprite))
            baseSpriteScale = sprite.Scale;

        enable = true;

        GD.Print(Name + " enable");
    }

    // ========================================================================
    // UPDATE
    // ========================================================================
    public override void _Process(double pDelta)
    {
        if (!enable) return;

        float lDelta = (float)pDelta;

        Update(lDelta);
    }

    protected virtual void Update(float pDelta) { }


    // ========================================================================
    // COLLIDE
    // ========================================================================

    protected virtual void OnCollide(Area2D pArea) { }
}
