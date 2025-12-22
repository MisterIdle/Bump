using Godot;
using System;
using System.Collections.Generic;

public partial class Pow : Projectiles
{
    [Export] private float scaleTime = 0.3f;
    [Export] private float scaleMultiplier = 1.2f;
    [Export] private float killDelay = 0.1f;

    private List<Enemy> enemyList;
    private List<Projectiles> projectilesList;
    private int index;

    public static Pow Create(PackedScene pScene, Vector2 pPos, Vector2 pWay)
    {
        Pow lPow = (Pow)pScene.Instantiate();
        lPow.Position = pPos;
        lPow.velocity = pWay;
        lPow.Initiate();
        return lPow;
    }

    protected override void Update(float pDelta)
    {
        base.Update(pDelta);
    }

    public override void Destory()
    {
        enemyList = Enemy.GetEnemiesOnScreen(GlobalPosition);
        projectilesList = GetEnemiesProjectilesOnScreen(GlobalPosition);
        index = 0;

        Tween lTween = CreateTween();
        lTween.TweenProperty(this, (string)PropertyName.Scale, Scale * scaleMultiplier, scaleTime)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.OutIn);

        lTween.Finished += Finish;
    }

    private void Finish()
    {
        if (enemyList.Count > 0)
            KillEnemy();
        else
            base.Destory();

        for (int i = projectilesList.Count - 1; i >= 0; i--)
            projectilesList[i].Destory();
    }

    private void KillEnemy()
    {
        if (index >= enemyList.Count)
        {
            base.Destory();
            return;
        }

        Enemy lEnemy = enemyList[index];
        lEnemy.health = 0;
        lEnemy.EntityHit();
        index++;

        Tween lTween = CreateTween();
        lTween.TweenCallback(Callable.From(KillEnemy))
                  .SetDelay(killDelay);
    }
}
