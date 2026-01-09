using Godot;

namespace Com.IsartDigital.Shmup.NManager;

public partial class AudioManager : Node
{
    private static AudioManager instance;

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

    [ExportGroup("Music")]
    [Export] private AudioStream uiMusic;
    [Export] private AudioStream levelMusic;
    [Export] private AudioStream bossMusic;

    private AudioStreamPlayer musicPlayer;

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
        musicPlayer.Bus = "Music";
        AddChild(musicPlayer);

        musicPlayer.Finished += OnMusicFinished;
    }

    // ========================================================================
    // SFX
    // ========================================================================

    public void Play(AudioType pType, float pPitch = 1.0f)
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

    private void PlayStream(AudioStream pStream, float pPitch)
    {
        AudioStreamPlayer lAudio = new AudioStreamPlayer();
        lAudio.Stream = pStream;
        lAudio.Bus = "SFX";

        if (pPitch <= 0f) pPitch = 1.0f;

        lAudio.PitchScale = pPitch;

        lAudio.Finished += lAudio.QueueFree;
        AddChild(lAudio);
        lAudio.Play();
    }

    // ========================================================================
    // MUSIC
    // ========================================================================

    public void PlayMusic(MusicType pType, float pFadeTime = 0.0f)
    {
        AudioStream lNewStream = ResolveMusic(pType);

        if (musicPlayer.Stream == lNewStream && musicPlayer.Playing) return;

        Tween lTween = CreateTween();

        lTween.TweenProperty(musicPlayer, (string)AudioStreamPlayer.PropertyName.VolumeDb, -80.0f, pFadeTime / 2)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.In);

        lTween.TweenCallback(Callable.From(() =>
        {
            musicPlayer.Stop();
            musicPlayer.Stream = lNewStream;
            if (lNewStream != null)
            {
                musicPlayer.VolumeDb = -80.0f;
                musicPlayer.Play();
            }
        }));

        if (lNewStream != null)
        {
            lTween.TweenProperty(musicPlayer, (string)AudioStreamPlayer.PropertyName.VolumeDb, 0.0f, pFadeTime / 2)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.Out);
        }
    }

    public void StopMusic(float pFadeTime = 1f)
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
            default: return null;
        }
    }

    public void SetMusicPaused(bool pIsPaused)
    {
        musicPlayer.StreamPaused = pIsPaused;
        GD.Print(musicPlayer.StreamPaused);
    }
}