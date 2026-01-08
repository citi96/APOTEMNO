using Godot;
using System;

namespace SacrificeMockup;

[GlobalClass]
public partial class ShepardTone : Node
{
    [Export] public float CycleDuration = 10.0f; // Seconds for one full octave drop
    [Export] public float BaseFrequency = 220.0f; // A3
    
    private AudioStreamPlayer[] _players;
    private AudioStreamGeneratorPlayback[] _playbacks;
    private float _time = 0f;

    // We use 3 oscillators spaced one octave apart
    // P1 starts at offset 0
    // P2 starts at offset 0.33 (relative to cycle) -> ensuring constant overlap
    // Standard Shepard uses specific bell curves.
    
    // Simpler Approach for Godot:
    // 3 Players always playing.
    // We modulate their PitchScale and VolumeDb every frame.

    public override void _Ready()
    {
        _players = new AudioStreamPlayer[3];
        _playbacks = new AudioStreamGeneratorPlayback[3];

        for (int i = 0; i < 3; i++)
        {
            var player = new AudioStreamPlayer();
            AddChild(player);
            _players[i] = player;

            var generator = new AudioStreamGenerator();
            generator.MixRate = 44100;
            generator.BufferLength = 0.5f; // Increased buffer to prevent crackling (Underruns)
            player.Stream = generator;
            player.Bus = "World"; // Route to Reverb Bus
            player.VolumeDb = -80.0f; // Start silent to avoid pop
            player.Play();
            
            _playbacks[i] = (AudioStreamGeneratorPlayback)player.GetStreamPlayback();
            
            // Fill buffer initially to prevent stutter
            FillBuffer(i); 
        }
    }

    public override void _Process(double delta)
    {
        _time += (float)delta;
        
        // Loop time to prevent overflow, though precision isn't critical here
        if (_time > CycleDuration) _time -= CycleDuration;

        float progress = _time / CycleDuration; // 0.0 to 1.0

        for (int i = 0; i < 3; i++)
        {
            // ... (Pitch logic unchanged) ...
            
            float T_i = (progress + (float)i/3.0f) % 1.0f;
            
            float exponent = 2.0f - (3.0f * T_i);
            float pitch = Mathf.Pow(2, exponent);
            
            _players[i].PitchScale = pitch;
            
            // Volume: Bell Curve (Hanning Window style)
            float vol = Mathf.Sin(T_i * Mathf.Pi);
            
            // Convert linear amplitude to db
            float db = Mathf.LinearToDb(vol);
            _players[i].VolumeDb = db - 25.0f; // Significantly lowered to ensure headroom
            
            // Continually fill buffer
            FillBuffer(i);
        }
    }

    private void FillBuffer(int index)
    {
        var playback = _playbacks[index];
        float sampleRate = 44100;
        
        // We just need a dirty sine wave
        // frames available
        int frames = playback.GetFramesAvailable();
        if (frames > 0)
        {
            var buffer = new Vector2[frames];
            float freq = BaseFrequency; // The pitch scale handles the drop
            float increment = freq / sampleRate;
            
            // We need persistent phase for each oscillator to avoid clicking?
            // Since we are changing PitchScale on the Player, the data in the buffer
            // is played back at variable speeds.
            // So we can just fill the buffer with a CONSTANT sine wave of BaseFrequency.
            // The Player's PitchScale will bend it.
            // This avoids phase discontinuities in generation logic!
            
            // Wait, if we keep filling, we need to track phase.
            // But since it's a constant frequency sine, phase is just time * freq.
            // Note: Each player needs its own phase tracker if we want it perfect, 
            // but actually, since we are resetting the sine loops, let's keep it simple.
            
            for (int i = 0; i < frames; i++)
            {
                // Just white noise?
                // No, sine.
                // Since PitchScale modulates the playback, we can feed a constant 220Hz tone
                // and the PitchScale will make it 440, 880, etc.
                
                // We need a phase for generation, otherwise each frame resets to 0 -> click.
                // We store phase in a static dictionary or array? 
                // Or just use a member variable array.
                
                // Hack: We don't have per-channel phase storage readily available in this scope loop structure 
                // unless I add it.
                // Let's add `_phases`.
                
                float phase = _phases[index];
                float val = Mathf.Sin(phase * Mathf.Pi * 2.0f);
                buffer[i] = new Vector2(val, val);
                
                phase += increment;
                if (phase > 1.0f) phase -= 1.0f;
                _phases[index] = phase;
            }
            playback.PushBuffer(buffer);
        }
    }
    
    private float[] _phases = new float[3];
}
