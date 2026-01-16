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

    public static bool IsOutOfBound(Vector2 pPos, float pMargin = 0, bool pCheckX = true, bool pCheckY = true)
    {
        Camera lCamera = Camera.GetInstance();

        if (lCamera == null)
            return false;

        Vector2 lCamPos = lCamera.GlobalPosition;
        Vector2 lSize = lCamera.GetViewportRect().Size;
        Vector2 lHalfSize = lSize / 2f;

        // Calcul des bords par rapport au centre de la caméra
        float lLeft = lCamPos.X - lHalfSize.X - pMargin;
        float lRight = lCamPos.X + lHalfSize.X + pMargin;

        float lTop = lCamPos.Y - lHalfSize.Y - pMargin;
        float lBottom = lCamPos.Y + lHalfSize.Y + pMargin;

        bool lIsOutX = pPos.X < lLeft || pPos.X > lRight;
        bool lIsOutY = pPos.Y < lTop || pPos.Y > lBottom;

        return (pCheckX && lIsOutX) || (pCheckY && lIsOutY);
    }

    // Dans Utils.cs
    public static Vector2 ClampToScreen(Vector2 pPos, float pMarginX = 0, float pMarginTop = 0, float pMarginBottom = 0)
    {
        Camera lCamera = Camera.GetInstance();

        if (lCamera == null)
            return pPos;

        Vector2 lCamPos = lCamera.GlobalPosition;
        Vector2 lSize = lCamera.GetViewportRect().Size;
        Vector2 lHalfSize = lSize / 2f;

        float lMinX = lCamPos.X - lHalfSize.X + pMarginX;
        float lMaxX = lCamPos.X + lHalfSize.X - pMarginX;

        float lMinY = lCamPos.Y - lHalfSize.Y + pMarginTop;
        float lMaxY = lCamPos.Y + lHalfSize.Y - pMarginBottom;

        if (lMinX > lMaxX) lMinX = lMaxX = lCamPos.X;
        if (lMinY > lMaxY) lMinY = lMaxY = lCamPos.Y;

        pPos.X = Mathf.Clamp(pPos.X, lMinX, lMaxX);
        pPos.Y = Mathf.Clamp(pPos.Y, lMinY, lMaxY);

        return pPos;
    }
}