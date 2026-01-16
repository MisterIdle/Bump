using Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;
using Com.IsartDigital.Shmup.NHUD;
using Com.IsartDigital.Shmup.NToolsBox;
using Godot;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class Walsy : Enemy
{
    private AnimatedSprite2D animatedSprite;

    [Export] private NodePath animatedSpritePath;
    [Export] private Color angryColor = Colors.Red;
    [Export] private bool angry;

    [Export] private float rotationSpeed;
    [Export] private float inverseRotationAmount = 15f;
    [Export] private float inverseRotationDuration = 0.2f;

    public override void Initiate()
    {
        base.Initiate();

        if (angry)
            Modulate = angryColor;

        animatedSprite = (AnimatedSprite2D)GetNode(animatedSpritePath);

        string lAnim = animatedSprite.Animation;
        health = animatedSprite.SpriteFrames.GetFrameCount(lAnim);
    }

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);
        RotationDegrees += rotationSpeed * pDelta;
    }

    private void InverseRotation()
    {
        Tween lInverseTween = CreateTween();
        lInverseTween.TweenProperty(this, (string)PropertyName.RotationDegrees, RotationDegrees - inverseRotationAmount, inverseRotationDuration);
    }

    public override void EntityHit(int pDamage = 1)
    {
        base.EntityHit(pDamage);

        if (!angry)
            animatedSprite.Frame++;

        Effect.BumpScale(this, baseScale);
        InverseRotation();
    }

    public override void EntityDeath()
    {
        base.EntityDeath();
        QueueFree();

        int lFinalScore = GetScoreWithCombo(killedByHammer ? score + hammerScore : score);

        HUD.GetInstance().CreateBubbleScore(Position, lFinalScore, Colors.Wheat);
    }

    protected override void OnCollide(Area2D pArea)
    {
        if (pArea is Projectiles lProjectiles && angry)
            return;

        base.OnCollide(pArea);
    }
}