using Godot;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class Koops : Enemy
{
    private enum TypeEnum { Green, Red }
    [Export] private TypeEnum type;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void Initiate()
    {
        base.Initiate();

        Color lTargetColor = (type == TypeEnum.Green) ? Colors.Green : Colors.Red;
        sprite.SelfModulate = lTargetColor;
    }

    public static Koops Create(PackedScene pScene, Vector2 pPos)
    {
        Koops lKoops = (Koops)pScene.Instantiate();
        lKoops.GlobalPosition = pPos;
        return lKoops;
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        if (type == TypeEnum.Green) 
            velocity.Y = angleY;
        else 
            velocity.X = angleX;
    }
}
