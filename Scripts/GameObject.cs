using Godot;
using System;

public partial class GameObject : Area2D
{
    public bool enable = false;

    public virtual void Initiate()
    {
        AreaEntered += OnCollide;

        enable = true;

        GD.Print(Name + " enable");
    }

    public override void _Process(double pDelta)
    {
        if (!enable) return;

        float lDelta = (float)pDelta;

        Update(lDelta);
    }

    protected virtual void Update(float pDelta) { }
    protected virtual void OnCollide(Area2D pArea) { }
}
