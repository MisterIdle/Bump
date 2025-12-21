using Godot;
using System;

public partial class Particule : GpuParticles2D
{
    public override void _Ready()
    {
        Emitting = true;
        Finished += QueueFree;
    }

    public static Particule Create(PackedScene pScene, Vector2 pPos)
    {
        Particule lParticule = (Particule)pScene.Instantiate();

        lParticule.Position = pPos;

        GameManager.GetInstance().AddChild(lParticule);
        return lParticule;
    }
}
