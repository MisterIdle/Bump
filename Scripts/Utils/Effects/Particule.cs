using Godot;

using Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;
using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NToolsBox;

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

        GameManager.GetInstance().gameContainer.AddChild(lParticule);
        lParticule.Position = pPos;

        return lParticule;
    }
}