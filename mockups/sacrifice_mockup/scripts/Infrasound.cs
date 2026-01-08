using Godot;
using System;

namespace SacrificeMockup;

[GlobalClass]
public partial class Infrasound : Node
{
    private AudioStreamPlayer _player;
    private AudioStreamGeneratorPlayback _playback;
    private float _time = 0f;
    private float _pulseTimer = 0f;

    [Export] public float Frequency = 32.0f; // 32Hz: Audible "Super Bass"
    [Export] public float PulseDuration = 5.0f; 
    
    public override void _Ready()
    {
        _player = new AudioStreamPlayer();
        AddChild(_player);
        
        var generator = new AudioStreamGenerator();
        generator.MixRate = 44100;
        generator.BufferLength = 0.5f;
        _player.Stream = generator;
        _player.Bus = "Infrasound"; 
        
        _player.VolumeDb = -80.0f; // Start absolutely silent
        _player.Play();
        
        _playback = (AudioStreamGeneratorPlayback)_player.GetStreamPlayback();
        FillBuffer();
        
        GD.Print("[INFRASOUND] Generator Started (32Hz Tuned Mode).");
    }

    public override void _Process(double delta)
    {
        _pulseTimer += (float)delta;
        if (_pulseTimer > PulseDuration) _pulseTimer -= PulseDuration; // Prevent float drift
        
        // Phase Offset to start at bottom of wave (-PI/2) at t=0
        float t = (_pulseTimer / PulseDuration) * Mathf.Pi * 2.0f;
        float shiftedT = t - (Mathf.Pi * 0.5f); 
        
        // Swell is 0..1
        float swell = (Mathf.Sin(shiftedT) + 1.0f) * 0.5f; 
        
        // Volume Range: -40dB (Silent) to -12dB (Safe Rumble)
        // Reduced peak from -2dB to avoid summing distortion
        float targetDb = Mathf.Lerp(-40.0f, -12.0f, swell);
        _player.VolumeDb = targetDb;
        
        FillBuffer();
        
        if (Engine.GetFramesDrawn() % 60 == 0)
            GD.Print($"[INFRASOUND] Pulse: {swell:F2} | DB: {targetDb:F1}");
    }

    private void FillBuffer()
    {
        int frames = _playback.GetFramesAvailable();
        if (frames > 0)
        {
            var buffer = new Vector2[frames];
            float increment = Frequency / 44100.0f;
            
            for (int i = 0; i < frames; i++)
            {
                _time += increment;
                if (_time > 1.0f) _time -= 1.0f;
                
                float sample = Mathf.Sin(_time * Mathf.Pi * 2.0f);
                buffer[i] = new Vector2(sample, sample);
            }
            _playback.PushBuffer(buffer);
        }
    }
}
