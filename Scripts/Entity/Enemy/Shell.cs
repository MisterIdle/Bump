using Godot;
using System;

public partial class Shell : GameObject
{
    [Export] private Sprite2D sprite;

    public void SetShellColor(Color pColor)
    {
        sprite.SelfModulate = pColor;
    }
}
