using Godot;
using System;

using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NToolsBox;

public partial class Particule : GpuParticles2D
{
    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        Emitting = true;
        Finished += QueueFree;
    }

    public static Particule Create(PackedScene pScene, Vector2 pPos)
    {
        Particule lParticule = (Particule)pScene.Instantiate();

        lParticule.Position = pPos;
        GameManager.GetInstance().gameContainer.AddChild(lParticule);

        return lParticule;
    }
}
