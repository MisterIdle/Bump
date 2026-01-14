using Godot;
using System;

namespace Com.IsartDigital.Shmup.NManager;

public partial class AudioManager : Node
{
    public enum AudioType
    {
        None,
        PlayerShoot,
        PlayerExplosion,
        PlayerLostLife,
        HammerCharge,
        HammerSwing,
        PowerUpPow,
        PowerLife,
        EnemyExplosion,
        EnemyShoot,
        BossShoot,
        BossPreExplosion,
        BossExplosion,
        UIClic,
        UIPause,
        UIStart
    }

    public enum MusicType
    {
        None,
        Menu,
        Level,
        Boss
    }

    [Export] private AudioStream ambiance;

    [ExportGroup("Player")]
    [Export] private AudioStream[] playerShoot;
    [Export] private AudioStream playerExplosion;
    [Export] private AudioStream playerLostLife;

    [ExportGroup("Hammer")]
    [Export] private AudioStream hammerCharge;

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

    [ExportGroup("Music")]
    [Export] private AudioStream uiMusic;
    [Export] private AudioStream levelMusic;
    [Export] private AudioStream bossMusic;

    private AudioStreamPlayer musicPlayer;
    private AudioStreamPlayer ambiancePlayer;

    private const string BUS_MUSIC = "Music";
    private const string BUS_SFX = "SFX";

    private const float DEFAULT_PITCH = 1.0f;
    private const float VOLUME_MUTED_DB = -80.0f;
    private const float VOLUME_NORMAL_DB = 0.0f;

    private const float DEFAULT_FADE_TIME = 0.0f;
    private const float STOP_MUSIC_FADE_TIME = 1f;
    private const int FADE_TIME_DIVIDER = 2;

    private static AudioManager instance;
    public static AudioManager GetInstance()
    {
        return instance;
    }

    // ========================================================================
    // INIT
    // ========================================================================

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
            return;
        }


        musicPlayer = new AudioStreamPlayer();
        musicPlayer.Bus = BUS_MUSIC;
        AddChild(musicPlayer);
        musicPlayer.Finished += OnMusicFinished;

        if (ambiance != null)
        {
            ambiancePlayer = new AudioStreamPlayer();
            ambiancePlayer.Stream = ambiance;

            ambiancePlayer.Bus = BUS_SFX;

            ambiancePlayer.Name = "AmbiancePlayer";

            AddChild(ambiancePlayer);

            ambiancePlayer.Finished += OnAmbianceFinished;

            ambiancePlayer.Play();
        }
    }

    // ========================================================================
    // SFX
    // ========================================================================

    public void Play(AudioType pType, float pPitch = DEFAULT_PITCH)
    {
        AudioStream lStream = ResolveStream(pType);
        if (lStream != null)
            PlayStream(lStream, pPitch);
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
        if (pList.Length > 0)
            return pList[GameManager.rand.RandiRange(0, pList.Length - 1)];
        return null;
    }

    private void PlayStream(AudioStream pStream, float pPitch)
    {
        AudioStreamPlayer lAudio = new AudioStreamPlayer();
        lAudio.Stream = pStream;
        lAudio.Bus = BUS_SFX;

        if (pPitch <= 0f)
            pPitch = DEFAULT_PITCH;

        lAudio.PitchScale = pPitch;

        lAudio.Finished += lAudio.QueueFree;
        AddChild(lAudio);
        lAudio.Play();
    }

    // ========================================================================
    // MUSIC
    // ========================================================================

    public void PlayMusic(MusicType pType, float pFadeTime = DEFAULT_FADE_TIME)
    {
        AudioStream lNewStream = ResolveMusic(pType);

        if (musicPlayer.Stream == lNewStream && musicPlayer.Playing) return;

        Tween lTween = CreateTween();
        string lPropertyVolume = (string)AudioStreamPlayer.PropertyName.VolumeDb;

        lTween.TweenProperty(musicPlayer, lPropertyVolume, VOLUME_MUTED_DB, pFadeTime / FADE_TIME_DIVIDER)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.In);

        lTween.TweenCallback(Callable.From(() =>
        {
            musicPlayer.Stop();
            musicPlayer.Stream = lNewStream;
            if (lNewStream != null)
            {
                musicPlayer.VolumeDb = VOLUME_MUTED_DB;
                musicPlayer.Play();
            }
        }));

        if (lNewStream != null)
        {
            lTween.TweenProperty(musicPlayer, lPropertyVolume, VOLUME_NORMAL_DB, pFadeTime / FADE_TIME_DIVIDER)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.Out);
        }
    }

    public void StopMusic(float pFadeTime = STOP_MUSIC_FADE_TIME)
    {
        PlayMusic(MusicType.None, pFadeTime);
    }

    private void OnMusicFinished()
    {
        musicPlayer.Play();
    }

    private AudioStream ResolveMusic(MusicType pType)
    {
        switch (pType)
        {
            case MusicType.Menu:
                return uiMusic;
            case MusicType.Level:
                return levelMusic;
            case MusicType.Boss:
                return bossMusic;
            default: 
                return null;
        }
    }

    public void SetMusicPaused(bool pIsPaused)
    {
        musicPlayer.StreamPaused = pIsPaused;
    }

    private void OnAmbianceFinished()
    {
        ambiancePlayer.Play();
    }
}