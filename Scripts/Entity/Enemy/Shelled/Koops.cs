using Godot;

using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class Koops : ShelledEnemy
{
    private enum TypeEnum { Green, Red, Blue, Purple }

    [Export] private TypeEnum type;

    public override void Initiate()
    {
        base.Initiate();

        Color lTargetColor = Colors.White;

        switch (type)
        {
            case TypeEnum.Green:
                lTargetColor = Colors.Green;
                break;
            case TypeEnum.Red:
                lTargetColor = Colors.Red;
                break;
            case TypeEnum.Blue:
                lTargetColor = Colors.Blue;
                break;
            case TypeEnum.Purple:
                lTargetColor = Colors.Purple;
                break;
        }

        shell.SetShellColor(lTargetColor);
    }

    public static Koops Create(PackedScene pScene, Vector2 pPos)
    {
        Koops lKoops = (Koops)pScene.Instantiate();

        GameManager.GetInstance().enemyContainer.AddChild(lKoops);

        lKoops.Position = pPos;
        lKoops.type = TypeEnum.Purple;

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
                break;
            default:
                break;
        }
    }
}
