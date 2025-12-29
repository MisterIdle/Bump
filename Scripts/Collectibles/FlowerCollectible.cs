using Godot;
using System;

public partial class FlowerCollectible : Collectibles
{
    [Export] private NodePath spritePath;

    private Sprite2D targetSprite;
    private const string COLOR_CONST = "New";

    private static ShaderMaterial material;
    private static Color sharedColor = Colors.White;
    private static int lastLevel = -1;

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
        FlowerCollectible collectible = (FlowerCollectible)pScene.Instantiate();
        collectible.Position = pPos;

        GameManager.GetInstance().gameContainer.AddChild(collectible);
        collectible.Initiate();

        return collectible;
    }

    protected override void OnCollide(Area2D pArea)
    {
        base.OnCollide(pArea);

        Player lPlayer = Player.GetInstance();
        lPlayer.LevelUp(true);

        int lLevel = lPlayer.level;
        if (lLevel != lastLevel)
        {
            lastLevel = lLevel;
            sharedColor = GetColorForLevel(lLevel);
            material.SetShaderParameter(COLOR_CONST, sharedColor);
        }

        Effect.SquashStretch(this, baseScale, true);
    }

    private Color GetColorForLevel(int level)
    {
        switch (level)
        {
            case 2: return Colors.White;
            case 3: return Colors.Red;
            default: return Colors.White;
        }
    }
}
