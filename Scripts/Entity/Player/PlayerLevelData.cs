using Godot;
using System;

namespace Com.IsartDigital.Shmup.NData;

[GlobalClass]
public partial class PlayerLevelData : Resource
{
    [ExportCategory("Weapon Configuration")]
    [Export] public PlayerShotData[] shots;
    [Export] public float[] angles;

    [ExportCategory("Player Visuals")]
    [Export] public Color playerColor = Colors.White;
    [Export] public float rotationSpeed = 15f;

    [ExportCategory("Player Stats")]
    [Export] public float moveSpeedMultiplier = 1.0f;
}