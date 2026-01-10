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
        Camera lCamera = Camera.GetInstance();

        if (lCamera == null) 
            return false;

        Vector2 lCamPos = lCamera.GlobalPosition;
        Vector2 lSize = lCamera.GetViewportRect().Size;

        float lLeft = lCamPos.X - pMargin;
        float lRight = lCamPos.X + lSize.X + pMargin;
        float lTop = lCamPos.Y - pMargin;
        float lBottom = lCamPos.Y + lSize.Y + pMargin;

        bool lIsOutX = pPos.X < lLeft || pPos.X > lRight;
        bool lIsOutY = pPos.Y < lTop || pPos.Y > lBottom;

        return (pCheckX && lIsOutX) || (pCheckY && lIsOutY);
    }

    public static Vector2 ClampToScreen(Vector2 pPos, float pMargin = 0)
    {
        Camera lCamera = Camera.GetInstance();

        if (lCamera == null) 
            return pPos;

        Vector2 lCamPos = lCamera.GlobalPosition;
        Vector2 lSize = lCamera.GetViewportRect().Size;

        float lMinX = lCamPos.X - pMargin;
        float lMaxX = lCamPos.X + lSize.X + pMargin;
        float lMinY = lCamPos.Y - pMargin;
        float lMaxY = lCamPos.Y + lSize.Y + pMargin;

        pPos.X = Mathf.Clamp(pPos.X, lMinX, lMaxX);
        pPos.Y = Mathf.Clamp(pPos.Y, lMinY, lMaxY);

        return pPos;
    }
}