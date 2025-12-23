using Godot;

public partial class Helmet : Sprite2D
{
    [Export] private Sprite2D baseSprite;

    [Export] private Texture2D levelB;
    [Export] private Texture2D levelC;

    [Export] private float levelBRotation = -90f;
    [Export] private Vector2 levelBPosition = Vector2.Zero;

    [Export] private float levelCRotation = 180f;
    [Export] private Vector2 levelCPosition = new Vector2(0, -20);

    public void SetLevel(int pLevel)
    {
        Effect.BumpScale(this);

        switch (pLevel)
        {
            case 1:
                Visible = false;
                break;

            case 2:
                Visible = true;
                baseSprite.Texture = levelB;
                baseSprite.RotationDegrees = levelBRotation;
                baseSprite.Position = levelBPosition;
                break;

            case 3:
                Visible = true;
                baseSprite.Texture = levelC;
                baseSprite.RotationDegrees = levelCRotation;
                baseSprite.Position = levelCPosition;
                break;
        }
    }
}
