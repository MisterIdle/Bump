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

    public static Vector2 OutOfBoundAxes(Vector2 pPos, float pMarginX = 0, float pMarginY = 0)
    {
        Vector2 lAxes = Vector2.Zero;

        Camera lCam = Camera.GetInstance();
        if (lCam == null) return lAxes;

        Vector2 lViewportSize = GameManager.GetInstance().GetViewportRect().Size;
        Vector2 lTopLeft = lCam.GetScreenCenterPosition() - (lViewportSize / 2);

        if (pPos.X < lTopLeft.X - pMarginX)
            lAxes.X = -1;
        else if (pPos.X > lTopLeft.X + lViewportSize.X + pMarginX)
            lAxes.X = 1;

        if (pPos.Y < lTopLeft.Y - pMarginY)
            lAxes.Y = -1;
        else if (pPos.Y > lTopLeft.Y + lViewportSize.Y + pMarginY)
            lAxes.Y = 1;

        return lAxes;
    }

    public static Vector2 ClampToScreen(Vector2 pPos, float pMarginX = 0, float pMarginY = 0)
    {
        Camera lCam = Camera.GetInstance();
        if (lCam == null) return pPos;

        Vector2 lViewportSize = GameManager.GetInstance().GetViewportRect().Size;
        Vector2 lTopLeft = lCam.GetScreenCenterPosition() - (lViewportSize / 2);

        float lClampX = Mathf.Clamp(pPos.X, lTopLeft.X + pMarginX, lTopLeft.X + lViewportSize.X - pMarginX);
        float lClampY = Mathf.Clamp(pPos.Y, lTopLeft.Y + pMarginY, lTopLeft.Y + lViewportSize.Y - pMarginY);

        return new Vector2(lClampX, lClampY);
    }
}