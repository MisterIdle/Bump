using Godot;
using System;

public partial class TickEvent : Node2D
{
    public enum BeatType
    {
        Attack,
        Shoot
    }

    [Signal] public delegate void BeatDetectedEventHandler(BeatType type);

    private const float SEUIL_VOLUME = 0.001f;
    private const double COOLDOWN = 0.14;

    private AudioEffectSpectrumAnalyzerInstance _attackAnalyzer;
    private double _timerAttack = 10.0;

    private AudioEffectSpectrumAnalyzerInstance _shootAnalyzer;
    private double _timerShoot = 10.0;

    public override void _Ready()
    {
        int attackBusIndex = AudioServer.GetBusIndex("TickAttack");
        _attackAnalyzer = (AudioEffectSpectrumAnalyzerInstance)AudioServer.GetBusEffectInstance(attackBusIndex, 0);

        int shootBusIndex = AudioServer.GetBusIndex("TickShoot");
        _shootAnalyzer = (AudioEffectSpectrumAnalyzerInstance)AudioServer.GetBusEffectInstance(shootBusIndex, 0);

        GD.Print("Audio Analysers initialisés !");
    }

    public override void _Process(double delta)
    {
        _timerAttack += delta;
        _timerShoot += delta;

        if (_timerAttack > COOLDOWN)
        {
            float volAttack = _attackAnalyzer.GetMagnitudeForFrequencyRange(0, 20000, AudioEffectSpectrumAnalyzerInstance.MagnitudeMode.Max).Length();

            if (volAttack > SEUIL_VOLUME)
            {
                _timerAttack = 0;
                OnBeat(BeatType.Attack);
            }
        }

        if (_timerShoot > COOLDOWN)
        {
            float volShoot = _shootAnalyzer.GetMagnitudeForFrequencyRange(0, 20000, AudioEffectSpectrumAnalyzerInstance.MagnitudeMode.Max).Length();

            if (volShoot > SEUIL_VOLUME)
            {
                _timerShoot = 0;
                OnBeat(BeatType.Shoot);
            }
        }
    }

    public void OnBeat(BeatType beatType)
    {
        EmitSignal(SignalName.BeatDetected, (int)beatType);

        switch (beatType)
        {
            case BeatType.Attack:
                GD.Print("ACTION: Lancement de l'attaque !");
                break;

            case BeatType.Shoot:
                GD.Print("ACTION: Lancement du shoot !");
                break;
        }
    }
}