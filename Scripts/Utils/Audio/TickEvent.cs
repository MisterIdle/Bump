using Godot;
using System;

public partial class TickEvent : Node2D
{
    public enum BeatTypeBoss
    {
        Dash,
        Shoot,
        Reload,
        Move,
        Phase,
    }

    [Signal] public delegate void BeatDetectedEventHandler(BeatTypeBoss type);

    [ExportGroup("Parameters")]
    [Export] public float seuilDb = -15.0f;
    [Export] public float cooldown = 0.12f;

    [ExportGroup("Audio Bus")]
    [Export] public string BusNameDash = "TickDash";
    [Export] public string BusNameShoot = "TickShoot";
    [Export] public string BusNameReload = "TickReload";
    [Export] public string BusNameMove = "TickMove";
    [Export] public string BusNamePhase = "TickPhase";

    private int indexDash;
    private int indexShoot;
    private int indexReload;
    private int indexMove;
    private int indexPhase;

    private double globalTimer = 0.5;

    public override void _Ready()
    {
        indexDash = AudioServer.GetBusIndex(BusNameDash);
        indexShoot = AudioServer.GetBusIndex(BusNameShoot);
        indexReload = AudioServer.GetBusIndex(BusNameReload);
        indexMove = AudioServer.GetBusIndex(BusNameMove);
        indexPhase = AudioServer.GetBusIndex(BusNamePhase);
    }

    public override void _Process(double delta)
    {
        globalTimer += delta;

        if (globalTimer < cooldown) return;

        if (CheckBusVolume(indexDash))
            OnBeat(BeatTypeBoss.Dash);

        if (CheckBusVolume(indexShoot))
            OnBeat(BeatTypeBoss.Shoot);

        if (CheckBusVolume(indexReload))
            OnBeat(BeatTypeBoss.Reload);

        if (CheckBusVolume(indexMove))
            OnBeat(BeatTypeBoss.Move);

        if (CheckBusVolume(indexPhase))
            OnBeat(BeatTypeBoss.Phase);
    }

    private bool CheckBusVolume(int pBusIndex)
    {
        float lDB = AudioServer.GetBusPeakVolumeLeftDb(pBusIndex, 0);
        return lDB > seuilDb;
    }

    public void OnBeat(BeatTypeBoss pBeatType)
    {
        globalTimer = 0;

        EmitSignal(SignalName.BeatDetected, (int)pBeatType);

        switch (pBeatType)
        {
            case BeatTypeBoss.Dash:
                GD.Print("ACTION: Dash");
                break;
            case BeatTypeBoss.Shoot:
                GD.Print("ACTION: Shoot");
                break;
            case BeatTypeBoss.Reload:
                GD.Print("ACTION: Reload");
                break;
            case BeatTypeBoss.Move:
                GD.Print("ACTION: Move");
                break;
            case BeatTypeBoss.Phase:
                GD.Print("ACTION: Phase");
                break;
        }
    }

    public void PlayAll()
    {
        foreach (Node lAudio in GetChildren())
        {
            if (lAudio is AudioStreamPlayer lAudioStream)
                lAudioStream.Play();
        }
    }
}