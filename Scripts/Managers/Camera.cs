using Godot;

public partial class Camera : Camera2D
{
    private static Camera instance;

    public static Camera GetInstance()
    {
        return instance;
    }

    public override void _Ready()
    {
        if (instance == null) instance = this;
        else
        {
            QueueFree();
            return;
        }
    }

    public override void _Process(double pDelta)
    {
        float lDelta = (float)pDelta;

        Position += GameManager.scrollSpeedVelocity * Time.GetDelta(lDelta);
    }

    public static void Shake(float pForce, float pTime, float pX = 1, float pY = 1, float pStep = 0.025f)
    {
        Camera pSelf = instance;
        pSelf.Offset = Vector2.Zero;

        int pSteps = Mathf.Max(1, (int)(pTime / pStep));
        float pStepDuration = pStep;


        Tween lTween = pSelf.CreateTween();

        for (int i = 0; i < pSteps; i++)
        {
            float lTime = 1f - (float)i / (pSteps - 1);

            Vector2 offset = new Vector2(
                GameManager.rand.RandfRange(-pX, pX),
                GameManager.rand.RandfRange(-pY, pY)
            ) * pForce * lTime;

            lTween.TweenProperty(pSelf, (string)PropertyName.Offset, offset, pStepDuration)
                  .SetTrans(Tween.TransitionType.Sine)
                  .SetEase(Tween.EaseType.InOut);
        }

        lTween.TweenProperty(pSelf, (string)PropertyName.Offset, Vector2.Zero, pStepDuration)
              .SetTrans(Tween.TransitionType.Sine)
              .SetEase(Tween.EaseType.InOut);
    }

    protected override void Dispose(bool pDisposing)
    {
        instance = null;
        base.Dispose(pDisposing);
    }
}
