using Godot;

namespace Com.IsartDigital.Shmup.NData;

[GlobalClass]
public partial class HammerData : Resource
{
    [Export] public float duration = 1.0f;

    [ExportGroup("Visuals")]
    [Export] public float targetChargeRotation = 0f;
    [Export] public float targetSwingRotation = 0f;
    [Export] public float scale = 1.0f;
    [Export] public Color barColor = Colors.White;
    [Export] public AudioStream sound;

    [ExportGroup("Stats")]
    [Export] public int force = 1;
    [Export] public float dashForce = 3f;
    [Export] public float swingDuration = 0.3f;
}