using Godot;
using System;
using System.Collections.Generic;

public partial class Pow : GameObject
{
    [Export] private float dropDuration = 1.0f;
    [Export] private float scaleMultiplier = 20f;
    [Export] private float killDelay = 0.01f;

    [Export] private float entranceOvershoot = -50f;
    [Export] private float entranceXOffsetTarget = 250f;
    [Export] private float minSpawnOffset = 100f;
    [Export] private PackedScene pop;

    private List<Enemy> enemyList;
    private List<Projectiles> projectilesList;
    private int index;

    private Vector2 targetPosition;

    public override void _Ready()
    {
        StartEntranceAnimation();
    }

    public static Pow Create(PackedScene pScene)
    {
        Pow lPow = (Pow)pScene.Instantiate();

        Vector2 screenCenter = GameManager.GetInstance().screenSizeCenterX;

        float spawnY = (GameManager.rand.Randf() < 0.5f) ? -lPow.minSpawnOffset : GameManager.GetInstance().screenSize.Y + lPow.minSpawnOffset;
        float spawnX = screenCenter.X;

        lPow.Position = new Vector2(spawnX, spawnY);

        float targetY = GameManager.GetInstance().screenSize.Y / 2;
        lPow.targetPosition = new Vector2(screenCenter.X + lPow.entranceXOffsetTarget, targetY);

        lPow.Initiate();

        return lPow;
    }

    private void StartEntranceAnimation()
    {
        Tween lTween = CreateTween();

        Vector2 overshootPos = targetPosition + new Vector2(0, entranceOvershoot);
        lTween.TweenProperty(this, (string)PropertyName.Position, overshootPos, dropDuration * 0.7f)
              .SetTrans(Tween.TransitionType.Sine)
              .SetEase(Tween.EaseType.Out);

        lTween.TweenProperty(this, (string)PropertyName.Position, targetPosition, dropDuration * 0.3f)
              .SetTrans(Tween.TransitionType.Bounce)
              .SetEase(Tween.EaseType.In);

        lTween.Finished += PostExplosion;
    }

    private void PostExplosion()
    {
        Tween lTween = Effect.SquashStretch(sprite, baseSpriteScale, false, true, 0.5f);

        Effect.FlashColor(this, Colors.Red);
        Camera.Shake(35, 1.2f);
        Time.SlowMotion();

        Particule.Create(pop, Position);

        lTween.Finished += StartExplosionSequence;
    }

    private void StartExplosionSequence()
    {
        // On récupère les listes
        enemyList = Enemy.GetEnemiesOnScreen(GlobalPosition);
        projectilesList = Projectiles.GetEnemiesProjectilesOnScreen(GlobalPosition);

        // --- CORRECTION DU CRASH ICI ---
        if (projectilesList != null)
        {
            for (int i = projectilesList.Count - 1; i >= 0; i--)
            {
                // On copie la référence locale pour éviter les accès multiples au tableau
                var proj = projectilesList[i];

                // On utilise GodotObject.IsInstanceValid (plus robuste que null check)
                if (IsInstanceValid(proj))
                {
                    // Au lieu d'appeler Destory() directement qui trigger QueueFree() tout de suite...
                    // On vérifie d'abord si l'objet n'est pas DEJA en train d'être free
                    if (!proj.IsQueuedForDeletion())
                    {
                        // Si ta méthode Destory fait juste QueueFree + particules, 
                        // appelle-la. Sinon, pour éviter le crash 100%, utilise:
                        proj.Destory();
                    }
                }
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
        // Sécurité : si la liste est devenue null ou vide entre temps
        if (enemyList == null || index >= enemyList.Count)
        {
            QueueFree();
            return;
        }

        // On boucle jusqu'à trouver un ennemi valide pour éviter de bloquer sur un ennemi déjà mort
        while (index < enemyList.Count)
        {
            Enemy lEnemy = enemyList[index];
            index++; // On incrémente tout de suite pour le prochain appel

            if (IsInstanceValid(lEnemy))
            {
                // Gestion du Boss
                if (lEnemy is KingKoops)
                {
                    // Le Boss prend des dégâts mais NE MEURT PAS forcément.
                    // IMPORTANT : On ne le retire pas de la liste manuellement ici,
                    // et comme on a fait index++, la boucle continuera vers les autres ennemis.
                    lEnemy.EntityHit(20);
                }
                else
                {
                    // Ennemi normal : Mort instantanée
                    lEnemy.health = 0;
                    lEnemy.EntityHit();
                }

                // On relance la boucle pour le suivant avec un délai
                Tween lTween = CreateTween();
                lTween.TweenCallback(Callable.From(KillNextEnemy))
                      .SetDelay(killDelay);
                return; // On sort pour attendre le Tween
            }
        }

        // Si on a fini la liste
        QueueFree();
    }
}
