using Godot;
using System;

public partial class Trigger : Area2D
{
    public void OnCollide(Area2D pArea)
    {
        if (pArea is GameObject lGameObject && !lGameObject.enable)
            lGameObject.Initiate();
    }
}
