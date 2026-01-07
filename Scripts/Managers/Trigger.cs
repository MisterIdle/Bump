using Godot;
using System;

using Com.IsartDigital.Shmup.NGameObject;

namespace Com.IsartDigital.Shmup.NManager;

public partial class Trigger : Area2D
{
    // ========================================================================
    // ACTIVATOR
    // ========================================================================

    public void OnCollide(Area2D pArea)
    {
        if (pArea is GameObject lGameObject && !lGameObject.enable)
            lGameObject.Initiate();
    }
}