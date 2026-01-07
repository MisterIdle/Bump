using Godot;
using System;

using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NToolsBox;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NCollectibles;

// CHECK

public partial class FlowerCollectible : Collectibles
{
    [Export] private NodePath spritePath;

    private Sprite2D targetSprite;
    private const string COLOR_CONST = "New";

    private static ShaderMaterial material;
    private static Color sharedColor = Colors.White;
    private static int lastLevel = -1;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void Initiate()
    {
        base.Initiate();

        targetSprite = GetNode<Sprite2D>(spritePath);

        material = (ShaderMaterial)targetSprite.Material;

        targetSprite.Material = material;

        material.SetShaderParameter(COLOR_CONST, sharedColor);
    }

    public static FlowerCollectible Create(PackedScene pScene, Vector2 pPos)
    {
        FlowerCollectible lCollectible = (FlowerCollectible)pScene.Instantiate();
        lCollectible.Position = pPos;

        GameManager.GetInstance().gameContainer.AddChild(lCollectible);
        lCollectible.Initiate();

        return lCollectible;
    }

    // ========================================================================
    // COLLIDE
    // ========================================================================

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        Player lPlayer = Player.GetInstance();
        lPlayer.LevelUp(true);

        int lLevel = lPlayer.level;
        if (lLevel != lastLevel)
        {
            lastLevel = lLevel;
            sharedColor = lLevel == 2 || lLevel == 3 ? Colors.Red : Colors.White; 
            material.SetShaderParameter(COLOR_CONST, sharedColor);
        }

        Effect.SquashStretch(this, baseScale, true);
    }
}
