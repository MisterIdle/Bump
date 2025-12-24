using Godot;
using System;
using System.Collections.Generic;

public partial class Pow : GameObject
{
    [Export] private float dropDuration = 1.0f;
    [Export] private float scaleMultiplier = 20f;
    [Export] private float killDelay = 0.01f;

    [Export] private PackedScene pop;

    private List<Enemy> enemyList;
    private List<Projectiles> projectilesList;
    private int index;

    private Vector2 targetPosition;
    private Vector2 baseScale;

    public override void _Ready()
    {
        StartEntranceAnimation();
        baseScale = Scale;
    }

    public static Pow Create(PackedScene pScene, Vector2 pPos)
    {
        Pow lPow = (Pow)pScene.Instantiate();

        Vector2 centerPos = GameManager.GetInstance().screenSizeCenterX;
        lPow.Position = new Vector2(centerPos.X, 10);

        lPow.targetPosition = new Vector2(centerPos.X + 80, 500);

        lPow.Initiate();

        return lPow;
    }

    private void StartEntranceAnimation()
    {
        RotationDegrees = 0;

        Tween lTween = CreateTween();

        lTween.SetParallel(true);

        lTween.TweenProperty(this, (string)PropertyName.Position, targetPosition, dropDuration)
            .SetTrans(Tween.TransitionType.Bounce)
            .SetEase(Tween.EaseType.Out);

        lTween.TweenProperty(this, (string)PropertyName.RotationDegrees, 360, dropDuration)
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.Out);

        lTween.TweenProperty(this, "scale", Scale * 4, dropDuration * 0.8f);

        lTween.SetParallel(false);

        lTween.Finished += PostExplosion;
    }

    private void PostExplosion()
    {
        Tween lTween = Effect.SquashStretch(this, false, true);

        Effect.FlashColor(this, Colors.Red);
        Camera.Shake(25, 1f);
        Time.SlowMotion(0.1f, 0.2f);

        Particule.Create(pop, Position);

        lTween.Finished += StartExplosionSequence;
    }

    private void StartExplosionSequence()
    {
        enemyList = Enemy.GetEnemiesOnScreen(GlobalPosition);
        projectilesList = Projectiles.GetEnemiesProjectilesOnScreen(GlobalPosition);

        if (projectilesList != null)
        {
            for (int i = projectilesList.Count - 1; i >= 0; i--)
            {
                if (IsInstanceValid(projectilesList[i]))
                    projectilesList[i].Destory();
            }
        }

        index = 0;

        if (enemyList != null && enemyList.Count > 0)
            KillNextEnemy();
        else
            QueueFree();
    }

    private void KillNextEnemy()
    {
        if (enemyList == null || index >= enemyList.Count)
        {
            QueueFree();
            return;
        }

        Enemy lEnemy = enemyList[index];

        lEnemy.health = 0;
        lEnemy.EntityHit();

        index++;

        Tween lTween = CreateTween();
        lTween.TweenCallback(Callable.From(KillNextEnemy))
              .SetDelay(killDelay);
    }
}