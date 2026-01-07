using Godot;
using System;

namespace Com.IsartDigital.Shmup.NToolsBox;

public partial class Pall : AnimatedSprite2D
{
    // ========================================================================
    // INIT
    // ========================================================================

    public override void _Ready()
    {
        base._Ready();
        Play();
    }
}
