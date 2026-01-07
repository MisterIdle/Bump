using Godot;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;

[GlobalClass]
public partial class HammerData : Resource
{
    [Export] public float Duration = 1.0f;

    [ExportGroup("Visuals")]
    [Export] public float targetChargeRotation = 0f;
    [Export] public float targetSwingRotation = 0f;
    [Export] public float scale = 1.0f;
    [Export] public Color barColor = Colors.White;

    [ExportGroup("Stats")]
    [Export] public int force = 1;
    [Export] public float dashForce = 3f;
    [Export] public float swingDuration = 0.3f;
}