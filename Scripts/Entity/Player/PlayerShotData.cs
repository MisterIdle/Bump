using Godot;

namespace Com.IsartDigital.Shmup.NData;

[GlobalClass]
public partial class PlayerShotData : Resource
{
    [ExportCategory("Projectile")]
    [Export] public PackedScene shotScene;
}