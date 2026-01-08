using Godot;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity;

namespace Com.IsartDigital.Shmup.NToolsBox;

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
        return Mathf.RadToDeg((pTo - pFrom).Angle());
    }

    // ========================================================================
    // SCREEN UTILS
    // ========================================================================

    public static bool IsOutOfBound(Vector2 pPos, float pMargin = 0, bool pCheckX = true, bool pCheckY = true)
    {
        Rect2 lScreen = Camera.GetInstance().GetScreenRect();

        if (pMargin != 0)
            lScreen = lScreen.Grow(pMargin);

        bool lIsOutX = pPos.X < lScreen.Position.X || pPos.X > lScreen.End.X;
        bool lIsOutY = pPos.Y < lScreen.Position.Y || pPos.Y > lScreen.End.Y;

        return (pCheckX && lIsOutX) || (pCheckY && lIsOutY);
    }

    public static Vector2 ClampToScreen(Vector2 pPos, float pMargin = 0)
    {
        Rect2 lScreen = Camera.GetInstance().GetScreenRect();

        if (pMargin != 0) 
            lScreen = lScreen.Grow(pMargin);

        pPos.X = Mathf.Clamp(pPos.X, lScreen.Position.X, lScreen.End.X);
        pPos.Y = Mathf.Clamp(pPos.Y, lScreen.Position.Y, lScreen.End.Y);

        return pPos;
    }
}