using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;
using Com.IsartDigital.Shmup.NHUD;
using Com.IsartDigital.Shmup.NManager;
using Com.IsartDigital.Shmup.NToolsBox;
using Godot;
using System.Collections.Generic;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;

public partial class ShelledEnemy : Enemy
{
    public enum FirePattern { Random, LeftToRight, RightToLeft }

    [ExportGroup("Base Settings")]
    [Export] public bool shellProjectileProtect;
    [Export] protected bool facePlayer;
    [Export] private NodePath shellPath;
    [Export] private NodePath wingPath;
    [Export] private NodePath spikeContainerPath;

    [ExportGroup("Spike System")]
    [Export] protected bool useSpikes = false;
    [Export] protected bool autoFire = true;
    [Export] protected FirePattern firePattern = FirePattern.Random;
    [Export] protected PackedScene spikeScene;
    [Export] protected Node2D spikeContainer;
    [Export] protected int numSpikes = 5;
    [Export] protected float spikeRadius = 25f;
    [Export] protected float fireRate = 0.5f;
    [Export] protected float reloadTime = 3.5f;
    [Export] protected float shotPreDelay = 0.5f;
    [Export] protected float spikeAdvanceMultiplier = 1.3f;
    [Export] protected float spikeSizeMultiplier = 1.2f;

    [Export] protected Color spikeBaseColor = Colors.White;
    [Export] protected Color spikeFlashColor = Colors.Red;

    protected List<Spike> spikesList = new List<Spike>();
    private List<Spike> chargingSpikesList = new List<Spike>();

    protected Timer spikeFireTimer;
    private Tween fireTween;
    private int lastShotIndex = -1;

    protected Shell shell;
    protected Wing wing;
    protected Vector2 playerDirection;
    protected Vector2 playerPos;
    protected bool faceLeft;

    private const float ANGLE_START_RATIO = -1f / 6f;
    private const float ANGLE_END_RATIO = -5f / 6f;
    private const float SPIKE_SCALE_DURATION = 0.2f;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void Initiate()
    {
        base.Initiate();

        spikeContainer = (Node2D)GetNodeOrNull(spikeContainerPath);
        shell = (Shell)GetNode(shellPath);
        wing = (Wing)GetNode(wingPath);

        if (useSpikes)
            SetupSpikeSystem();
    }

    protected void SetupSpikeSystem()
    {
        spikeFireTimer = new Timer();

        spikeFireTimer.WaitTime = reloadTime;
        spikeFireTimer.OneShot = true;
        spikeFireTimer.Timeout += TryFireSpike;

        AddChild(spikeFireTimer);
    }

    // ========================================================================
    // UPDATE
    // ========================================================================

    protected override void AliveUpdate(float pDelta)
    {
        base.AliveUpdate(pDelta);

        playerPos = Player.GetInstance().GlobalPosition;
        playerDirection = Utils.DirectionTo(GlobalPosition, playerPos);

        if (facePlayer)
        {
            bool lFaceLeft = playerDirection.X < 0;
            if (lFaceLeft != faceLeft)
            {
                faceLeft = lFaceLeft;
                Flip(faceLeft);
            }
        }
    }

    // ========================================================================
    // ATTACK
    // ========================================================================

    public void StartSpike()
    {
        if (!useSpikes) return;

        CallDeferred(MethodName.SpawnSpikes);

        if (autoFire)
            spikeFireTimer.Start(reloadTime);
    }

    public void StopSpike()
    {
        spikeFireTimer?.Stop();
    }

    protected void SpawnSpikes()
    {
        chargingSpikesList.Clear();

        float lStartAngle = Mathf.Pi * ANGLE_START_RATIO;
        float lEndAngle = Mathf.Pi * ANGLE_END_RATIO;
        float lAngleStep = 0;

        lAngleStep = (lEndAngle - lStartAngle) / (numSpikes - 1);

        for (int i = 0; i < numSpikes; i++)
        {
            if (i < spikesList.Count && IsInstanceValid(spikesList[i]) && !spikesList[i].isActive)
                continue;

            float lAngle = lStartAngle + (i * lAngleStep);
            Vector2 lPos = new Vector2(Mathf.Cos(lAngle) * spikeRadius, Mathf.Sin(lAngle) * spikeRadius);

            Spike lSpike = Spike.Create(spikeContainer, spikeScene, Player.GetInstance(), lPos);
            lSpike.Rotation = lAngle;
            lSpike.sprite.SelfModulate = spikeBaseColor;

            if (i < spikesList.Count)
                spikesList[i] = lSpike;
            else
                spikesList.Add(lSpike);

            lSpike.lifeTimer.Stop();

            lSpike.Position = Vector2.Zero;

            CreateTween().TweenProperty(lSpike, (string)PropertyName.Position, lPos * spikeAdvanceMultiplier, shotPreDelay)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
        }
    }

    protected void TryFireSpike()
    {
        if (isDeath) return;

        List<int> lReadyList = new List<int>();

        for (int i = 0; i < spikesList.Count; i++)
        {
            if (IsInstanceValid(spikesList[i]) && !spikesList[i].isActive && !chargingSpikesList.Contains(spikesList[i]))
                lReadyList.Add(i);
        }

        if (lReadyList.Count == 0)
        {
            int lActiveOrCharging = 0;

            foreach (Spike lSpike in spikesList)
            {
                if (lSpike != null && (lSpike.isActive || chargingSpikesList.Contains(lSpike)))
                    lActiveOrCharging++;
            }

            if (lActiveOrCharging == spikesList.Count)
            {
                bool lAllFire = true;

                foreach (Spike lSpike in spikesList)
                {
                    if (lSpike != null && !lSpike.isActive)
                        lAllFire = false;
                }

                if (lAllFire)
                {
                    SpawnSpikes();
                    if (autoFire)
                        spikeFireTimer.Start(reloadTime);
                    return;
                }
            }

            if (autoFire)
                spikeFireTimer.Start(reloadTime);
            return;
        }

        int lSelectedSpikeIndex = -1;

        switch (firePattern)
        {
            case FirePattern.Random:
                lSelectedSpikeIndex = lReadyList[GameManager.rand.RandiRange(0, lReadyList.Count - 1)];
                break;

            case FirePattern.LeftToRight:
                lSelectedSpikeIndex = lReadyList[lReadyList.Count - 1];
                break;

            case FirePattern.RightToLeft:
                lSelectedSpikeIndex = lReadyList[0];
                break;
        }

        lastShotIndex = lSelectedSpikeIndex;

        FireSpike(spikesList[lSelectedSpikeIndex]);

        if (autoFire)
            spikeFireTimer.Start(fireRate);
    }

    protected void FireSpike(Spike pSpike)
    {
        if (isDeath || !IsInstanceValid(pSpike)) return;

        chargingSpikesList.Add(pSpike);

        Tween lFireTween = CreateTween();

        lFireTween.TweenProperty(pSpike.sprite, (string)CanvasItem.PropertyName.SelfModulate, spikeFlashColor, shotPreDelay)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);

        lFireTween.Finished += () => LaunchSpike(pSpike);
    }

    private void LaunchSpike(Spike pSpike)
    {
        if (chargingSpikesList.Contains(pSpike))
            chargingSpikesList.Remove(pSpike);

        if (!IsInstanceValid(pSpike) || pSpike.isActive) return;

        pSpike.isActive = true;
        pSpike.sprite.SelfModulate = spikeFlashColor;

        AudioManager.GetInstance().Play(AudioManager.AudioType.EnemyShoot);

        Vector2 lGlobalPos = pSpike.GlobalPosition;
        float lGlobalRot = pSpike.GlobalRotation;

        spikeContainer.RemoveChild(pSpike);
        GameManager.GetInstance().projectilesContainer.AddChild(pSpike);

        pSpike.GlobalPosition = lGlobalPos;
        pSpike.GlobalRotation = lGlobalRot;

        pSpike.lifeTimer.Start();
        pSpike.particles.Emitting = true;
        pSpike.collision.Disabled = false;

        Tween lTween = CreateTween();
        lTween.TweenProperty(pSpike, (string)PropertyName.Scale, pSpike.Scale * spikeSizeMultiplier, SPIKE_SCALE_DURATION);
    }

    // ========================================================================
    // COLLIDE
    // ========================================================================

    public override void EntityHit(int pDamage)
    {
        base.EntityHit(pDamage);

        if (health <= 0)
        {
            shell?.ShellDestructionAnimation();
            wing?.SetSpeedDouble(true);
        }
    }

    public override void EntityDeath()
    {
        base.EntityDeath();

        StopSpike();
        fireTween?.Kill();
        chargingSpikesList.Clear();

        if (killedByHammer)
            HUD.GetInstance().CreateBubbleScore(Position, score + hammerScore, shell.GetShellColor());
        else
            HUD.GetInstance().CreateBubbleScore(Position, score, shell.GetShellColor());
    }

    protected override void OnCollide(Area2D pArea)
    {
        if (!shellProjectileProtect || pArea.Owner is Hammer)
        {
            base.OnCollide(pArea);
            shell?.ShellDamageAnimation();
        }

        if (pArea is Projectiles)
        {
            shell?.ShellProtectAnimation();
            return;
        }

        if (pArea is Player lPlayer && lPlayer.invincible)
        {
            shell?.ShellProtectAnimation();
            return;
        }
    }
}
