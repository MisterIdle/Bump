using Godot;

public partial class Koops : Enemy
{
    private enum TypeEnum
    {
        Green,
        Red
    }

    private float elapsedTime;

    [Export] private float frequency = 1.0f;
    [Export] private float amplitude = 1.0f;

    [Export] private TypeEnum type;

    private Vector2 baseDirection;

    public override void Initiate()
    {
        base.Initiate();

        velocity = Vector2.Left;

        switch (type)
        {
            case TypeEnum.Green:
                shell.SetShellColor(Colors.Green);
                break;

            case TypeEnum.Red:
                shell.SetShellColor(Colors.Red);
                break;
        }
    }

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);

        elapsedTime += pDelta;

        float lAngleX = Mathf.Sin(elapsedTime * frequency) * amplitude;
        float lAngleY = Mathf.Cos(elapsedTime * frequency) * amplitude;

        switch (type)
        {
            case TypeEnum.Green:
                velocity.Y = lAngleY * amplitude;
                break;

            case TypeEnum.Red:
                velocity.X = lAngleX * amplitude;
                break;
        }
    }

}
