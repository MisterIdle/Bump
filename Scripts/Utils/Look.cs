using Godot;
public static class Look
{
    public static Vector2 DirectionTo(Movable pFrom, Movable pTo)
    {
        return (pTo.GlobalPosition - pFrom.GlobalPosition).Normalized();
    }

    public static float DistanceTo(Entity pEntityA, Entity pEntityB)
    {
        return (pEntityA.GlobalPosition - pEntityB.GlobalPosition).Length();
    }

    public static float LookAt(Vector2 pFrom, Vector2 pTo)
    {
        float lAngle = (pTo - pFrom).Angle();

        return Mathf.RadToDeg(lAngle);
    }
}
