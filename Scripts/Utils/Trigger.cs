using Godot;
using System;

public partial class Trigger : GameObject
{
    protected override void OnCollide(Area2D pArea)
    {
        if (pArea is GameObject lGameObject && !lGameObject.enable)
            lGameObject.Initiate();
    }
}
