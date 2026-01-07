using Godot;

namespace Com.IsartDigital.Shmup.NData;

[GlobalClass]
public partial class PlayerShotData : Resource
{
    [ExportCategory("Projectile")]
    [Export] public PackedScene shotScene;

    [ExportCategory("Parameters")]
    [Export] public float speed = 600f;
    [Export] public float lifeTime = 4f;

    [ExportCategory("Pattern")]
    [Export] public float spreadAngle = 15f;
    [Export] public float minSpaceBetweenShots = 200f;
}