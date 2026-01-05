using Godot;
using System.Collections.Generic;

using Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NPlayer;
using Com.IsartDigital.Shmup.NGameObject.NMovable.NProjectiles;
using Com.IsartDigital.Shmup.NToolsBox;
using Com.IsartDigital.Shmup.NManager;

namespace Com.IsartDigital.Shmup.NGameObject.NMovable.NEntity.NEnemy;
public partial class ShelledEnemy : Enemy
{
    [ExportGroup("Base Settings")]
    [Export] public bool shellProjectileProtect;
    [Export] protected bool facePlayer;
    [Export] protected NodePath shellPath;
    [Export] protected NodePath wingPath;

    [ExportGroup("Shared Spike System")]
    [Export] protected bool useSpikes = false;
    [Export] protected PackedScene spikeScene;
    [Export] protected Node2D spikeContainer;
    [Export] protected int numSpikes = 5;
    [Export] protected float spikeRadius = 25f;
    [Export] protected float fireRate = 3.5f;
    [Export] protected float shotPreDelay = 0.5f;
    [Export] protected float spikeAdvanceMultiplier = 1.3f;
    [Export] protected float spikeSizeMultiplier = 1.2f;

    [Export] protected Color spikeBaseColor = Colors.White;
    [Export] protected Color spikeFlashColor = Colors.Red;

    protected List<Spike> spikesList = new List<Spike>();
    private List<Spike> chargingSpikes = new List<Spike>();

    protected Timer spikeAutoFireTimer;
    private Tween fireTween;
    private int lastShotIndex = -1;

    protected Shell shell;
    protected Wing wing;
    protected Vector2 playerDirection;
    protected Vector2 playerPos;
    protected bool faceLeft;

    // ========================================================================
    // INIT
    // ========================================================================

    public override void Initiate()
    {
        base.Initiate();

        shell = GetNodeOrNull<Shell>(shellPath);
        wing = GetNodeOrNull<Wing>(wingPath);

        if (useSpikes)
            SetupSpikeSystem();
    }

    protected void SetupSpikeSystem()
    {
        spikeAutoFireTimer = new Timer();
        spikeAutoFireTimer.WaitTime = fireRate;
        spikeAutoFireTimer.OneShot = true;
        spikeAutoFireTimer.Timeout += TryFireRandomSpike;
        AddChild(spikeAutoFireTimer);
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

    public void StartSpikeTimer()
    {
        if (!useSpikes) return;
        CallDeferred(MethodName.SpawnSpikes);
        spikeAutoFireTimer.Start();
    }

    public void StopSpikeRoutine()
    {
        spikeAutoFireTimer?.Stop();
    }

    protected void SpawnSpikes()
    {
        chargingSpikes.Clear();

        float startAngle = -Mathf.Pi / 6;
        float endAngle = 5 * -Mathf.Pi / 6;
        float angleStep = 0;

        if (numSpikes > 1)
            angleStep = (endAngle - startAngle) / (numSpikes - 1);
        else
            startAngle = -Mathf.Pi / 2;

        for (int i = 0; i < numSpikes; i++)
        {
            if (i < spikesList.Count && IsInstanceValid(spikesList[i]) && !spikesList[i].isActive)
                continue;

            float angle = startAngle + (i * angleStep);
            Vector2 localPos = new Vector2(Mathf.Cos(angle) * spikeRadius, Mathf.Sin(angle) * spikeRadius);

            Spike lSpike = Spike.Create(spikeScene, Player.GetInstance(), localPos);
            lSpike.Rotation = angle;
            lSpike.sprite.SelfModulate = spikeBaseColor;

            if (i < spikesList.Count) spikesList[i] = lSpike;
            else spikesList.Add(lSpike);

            spikeContainer.AddChild(lSpike);
            lSpike.lifeTimer.Stop();

            lSpike.Position = Vector2.Zero;
            CreateTween().TweenProperty(lSpike, "position", localPos * spikeAdvanceMultiplier, shotPreDelay)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        }
    }

    protected void TryFireRandomSpike()
    {
        if (isDeath) return;

        List<int> readyIndices = new List<int>();
        for (int i = 0; i < spikesList.Count; i++)
        {
            if (IsInstanceValid(spikesList[i]) && !spikesList[i].isActive && !chargingSpikes.Contains(spikesList[i]))
                readyIndices.Add(i);
        }

        if (readyIndices.Count == 0)
        {
            int activeOrChargingCount = 0;
            foreach (var s in spikesList) if (s != null && (s.isActive || chargingSpikes.Contains(s))) activeOrChargingCount++;

            if (activeOrChargingCount == spikesList.Count)
            {
                bool allFired = true;
                foreach (var s in spikesList) if (s != null && !s.isActive) allFired = false;

                if (allFired)
                {
                    SpawnSpikes();
                    spikeAutoFireTimer.Start(0.5f);
                    return;
                }
            }

            spikeAutoFireTimer.Start(0.1f);
            return;
        }

        int randomIndex = readyIndices[GameManager.rand.RandiRange(0, readyIndices.Count - 1)];

        if (readyIndices.Count > 1 && randomIndex == lastShotIndex)
            randomIndex = readyIndices[(readyIndices.IndexOf(randomIndex) + 1) % readyIndices.Count];

        lastShotIndex = randomIndex;
        FireSpikeSequence(spikesList[randomIndex]);

        spikeAutoFireTimer.Start(fireRate);
    }

    protected void FireSpikeSequence(Spike spike)
    {
        if (isDeath || !IsInstanceValid(spike)) return;

        chargingSpikes.Add(spike);

        Tween lTween = CreateTween();
        lTween.TweenProperty(spike.sprite, "self_modulate", spikeFlashColor, shotPreDelay)
         .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        lTween.Finished += () => LaunchSpike(spike);
    }

    private void LaunchSpike(Spike spike)
    {
        if (chargingSpikes.Contains(spike))
            chargingSpikes.Remove(spike);

        if (!IsInstanceValid(spike) || spike.isActive) return;

        spike.isActive = true;
        spike.sprite.SelfModulate = spikeFlashColor;

        AudioManager.GetInstance().Play(AudioManager.AudioType.EnemyShoot);

        Vector2 globalPos = spike.GlobalPosition;
        float globalRot = spike.GlobalRotation;

        spikeContainer.RemoveChild(spike);
        GameManager.GetInstance().gameContainer.AddChild(spike);

        spike.GlobalPosition = globalPos;
        spike.GlobalRotation = globalRot;

        spike.lifeTimer.Start();
        spike.particles.Emitting = true;
        spike.collision.Disabled = false;

        CreateTween().TweenProperty(spike, "scale", spike.Scale * spikeSizeMultiplier, 0.2f);
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

        StopSpikeRoutine();
        fireTween?.Kill();
        chargingSpikes.Clear();
    }

    protected override void OnCollide(Area2D pArea)
    {
        if ((pArea is Projectiles || (pArea is Player lPlayer && lPlayer.invincible)) && shellProjectileProtect)
        {
            shell?.ShellProtectAnimation();
            return;
        }

        base.OnCollide(pArea);
        shell?.ShellDamageAnimation();
    }
}
