using Godot;
using System;

public partial class SceneManager : Node
{
    [Export] private PackedScene scene1;

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(Property.INPUT_RELOAD))
            GetTree().ReloadCurrentScene();
    }
}
