using Godot;
using System;

public partial class AudioManager : Node
{
    public static AudioManager instance;

    public enum AudioType
    {
        None,

        // Player
        PlayerShoot,
        PlayerExplosion,
        PlayerLostLife,

        // Hammer Spécifique
        HammerCharge,
        HammerSwing,

        // PowerUps
        PowerUpPow,
        PowerLife,

        // Enemies
        EnemyExplosion,
        EnemyShoot,

        // Boss
        BossShoot,
        BossPreExplosion,
        BossExplosion,

        // UI
        UIClic,
        UIPause,
        UIStart
    }

    [ExportGroup("Player")]
    [Export] private AudioStream[] playerShoot;
    [Export] private AudioStream playerExplosion;
    [Export] private AudioStream playerLostLife;

    [ExportGroup("Hammer")]
    [Export] private AudioStream hammerCharge;
    [Export] private AudioStream hammerSwing;

    [ExportGroup("PowerUps")]
    [Export] private AudioStream powerUpPow;
    [Export] private AudioStream powerLife;

    [ExportGroup("Enemy")]
    [Export] private AudioStream[] enemyExplosion;
    [Export] private AudioStream enemyShoot;

    [ExportGroup("Boss")]
    [Export] private AudioStream bossShoot;
    [Export] private AudioStream bossPreExplosion;
    [Export] private AudioStream bossExplosion;

    [ExportGroup("UI")]
    [Export] private AudioStream uiClic;
    [Export] private AudioStream uiPause;
    [Export] private AudioStream uiStart;

    public override void _Ready()
    {
        if (instance == null)
        {
            instance = this;
            ProcessMode = ProcessModeEnum.Always;
        }
        else
        {
            QueueFree();
        }
    }

    public static AudioManager GetInstance()
    {
        return instance;
    }

    public void Play(AudioType pType, float pPitchVariance = 0f)
    {
        AudioStream lStream = ResolveStream(pType);
        if (lStream != null)
            PlayStream(lStream, pPitchVariance);
    }

    public AudioStream GetStream(AudioType pType)
    {
        return ResolveStream(pType);
    }

    private AudioStream ResolveStream(AudioType pType)
    {
        switch (pType)
        {
            case AudioType.PlayerShoot: return Pick(playerShoot);
            case AudioType.PlayerExplosion: return playerExplosion;
            case AudioType.PlayerLostLife: return playerLostLife;

            case AudioType.HammerCharge: return hammerCharge;
            case AudioType.HammerSwing: return hammerSwing;

            case AudioType.PowerUpPow: return powerUpPow;
            case AudioType.PowerLife: return powerLife;

            case AudioType.EnemyExplosion: return Pick(enemyExplosion);
            case AudioType.EnemyShoot: return enemyShoot;

            case AudioType.BossShoot: return bossShoot;
            case AudioType.BossPreExplosion: return bossPreExplosion;
            case AudioType.BossExplosion: return bossExplosion;

            case AudioType.UIClic: return uiClic;
            case AudioType.UIPause: return uiPause;
            case AudioType.UIStart: return uiStart;

            default: return null;
        }
    }

    private AudioStream Pick(AudioStream[] pList)
    {
        if (pList != null && pList.Length > 0)
            return pList[GD.RandRange(0, pList.Length - 1)];
        return null;
    }

    private void PlayStream(AudioStream pStream, float pPitchVariance)
    {
        var lPlayer = new AudioStreamPlayer();
        lPlayer.Stream = pStream;
        lPlayer.Bus = "SFX";

        if (pPitchVariance > 0)
        {
            lPlayer.PitchScale = pPitchVariance;
        }

        lPlayer.Finished += lPlayer.QueueFree;
        AddChild(lPlayer);
        lPlayer.Play();
    }
}