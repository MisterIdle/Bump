using Com.IsartDigital.Shmup.NManager;
using Godot;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class Koops : ShelledEnemy
{
    private enum TypeEnum { Green, Red, Blue }

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
        }

        shell.SetShellColor(lTargetColor);
    }

    public static Koops Create(PackedScene pScene, Vector2 pPos)
    {
        Koops lKoops = (Koops)pScene.Instantiate();

        GameManager.GetInstance().enemyContainer.AddChild(lKoops);

        lKoops.GlobalPosition = pPos;
        lKoops.type = TypeEnum.Green;
        lKoops.facePlayer = true;

        return lKoops;
    }

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        if (facePlayer)
            direction = faceLeft ? direction = MoveDirection.Left : direction = MoveDirection.Right;

        if (type == TypeEnum.Green)
            velocity.Y = angleY;
        else if (type == TypeEnum.Red)
            velocity.X = angleX;
    }

}
