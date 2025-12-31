using Godot;

public static class Utils
{
    public static Vector2 DirectionTo(Vector2 pFrom, Vector2 pTo)
    {
        return (pTo - pFrom).Normalized();
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

    public static Vector2 OutOfBoundAxes(Vector2 pPos, float pMarginX = 0, float pMarginY = 0)
    {
        Vector2 lAxes = Vector2.Zero;
        Vector2 lCamPos = Camera.GetInstance().Position;
        Vector2 lScreenSize = GameManager.GetInstance().GetViewportRect().Size;
    
        if (pPos.X < lCamPos.X - pMarginX)
            lAxes.X = -1;
        else if (pPos.X > lCamPos.X + lScreenSize.X + pMarginX)
            lAxes.X = 1;
    
        if (pPos.Y < lCamPos.Y - pMarginY)
            lAxes.Y = -1;
        else if (pPos.Y > lCamPos.Y + lScreenSize.Y + pMarginY)
            lAxes.Y = 1;
    
        return lAxes;
    }

    public static Vector2 ClampToScreen(Vector2 pPos, float pMarginX = 0, float pMarginY = 0)
    {
        Vector2 lCamPos = Camera.GetInstance().Position;
        Vector2 lScreenSize = GameManager.GetInstance().GetViewportRect().Size;

        float lClampX = Mathf.Clamp(pPos.X, lCamPos.X - pMarginX, lCamPos.X + lScreenSize.X + pMarginX);
        float lClampY = Mathf.Clamp(pPos.Y, lCamPos.Y - pMarginY, lCamPos.Y + lScreenSize.Y + pMarginY);

        return new Vector2(lClampX, lClampY);
    }

}
