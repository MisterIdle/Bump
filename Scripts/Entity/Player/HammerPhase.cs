using Godot;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;

[GlobalClass]
public partial class HammerPhase : Resource
{
    [Export] public float Duration = 1.0f;

    [ExportGroup("Visuals")]
    [Export] public float TargetChargeRotation = -20f;
    [Export] public float TargetSwingRotation = -120f;
    [Export] public float Scale = 1.0f;
    [Export] public Color BarColor = Colors.Orange;

    [ExportGroup("Stats")]
    [Export] public int Force = 10;
    [Export] public float DashForce = 150f;
    [Export] public float SwingDuration = 0.2f;
}