using Godot;

using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class Koops : ShelledEnemy
{
    private enum TypeEnum { Green, Red, Blue }

    [Export] private TypeEnum type;

    [Export] private float redSpeed = 200;
    [Export] private float greenSpeed = 100;
    [Export] private float blueSpeed = 50;

    public override void Initiate()
    {
        base.Initiate();

        Color lTargetColor = Colors.White;

        switch (type)
        {
            case TypeEnum.Green:
                lTargetColor = Colors.Green;
                speed = greenSpeed;
                break;
            case TypeEnum.Red:
                lTargetColor = Colors.Red;
                speed = redSpeed;
                break;
            case TypeEnum.Blue:
                lTargetColor = Colors.Blue;
                speed = blueSpeed;
                break;
        }

        shell.SetShellColor(lTargetColor);
    }

    public static Koops Create(PackedScene pScene, Vector2 pPos)
    {
        Koops lKoops = (Koops)pScene.Instantiate();

        GameManager.GetInstance().enemyContainer.AddChild(lKoops);

        lKoops.Position = pPos;
        lKoops.type = TypeEnum.Blue;

        return lKoops;
    }

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        switch (type)
        {
            case TypeEnum.Green:
                break;
            case TypeEnum.Red:
                velocity.Y = angleY;
                break;
            case TypeEnum.Blue:
                velocity = playerDirection;
                break;
            default:
                break;
        }
    }
}
