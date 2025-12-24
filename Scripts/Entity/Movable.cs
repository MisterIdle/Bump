using Godot;
using System;

public partial class Movable : GameObject
{
    [Export] public float speed;
    [Export] public bool affectedByScroll;

    protected Vector2 velocity;

    protected override void Update(float pDelta)
    {
        pDelta = Time.GetDelta(pDelta);

        Vector2 lTotalVelocity = Vector2.Zero;
        if (affectedByScroll)
            lTotalVelocity += GameManager.scrollSpeedVelocity;

        lTotalVelocity += velocity * speed;
        Position += lTotalVelocity * pDelta;
    }
}
