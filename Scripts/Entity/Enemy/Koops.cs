using Godot;

public partial class Koops : ShelledEnemy
{
    private enum TypeEnum
    {
        Green,
        Red
    }

    [Export] private TypeEnum type;

    public override void Initiate()
    {
        base.Initiate();

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

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        switch (type)
        {
            case TypeEnum.Green:
                velocity.Y = angleY * amplitude;
                break;

            case TypeEnum.Red:
                velocity.X = angleX * amplitude;
                break;
        }
    }
}
