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
            generator.BufferLength = 0.1f; // Short buffer for low latency
            player.Stream = generator;
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
            // Offset each voice by 1/3 of a cycle? 
            // Better: Offset by Octaves. 
            // A Shepard tone usually sweeps a range of ~10 octaves, or simplified 3.
            // Let's model "Virtual Pitch".
            
            // Basic Logic:
            // We want the Pitch to drop from 2.0 to 1.0 (or 4.0 to 0.5).
            // Let's say we have 3 voices.
            // Voice A: 4.0 -> 2.0
            // Voice B: 2.0 -> 1.0
            // Voice C: 1.0 -> 0.5
            // But we need them to loop.
            
            // Continuous Logic: 
            // Pitch(t) = 2^( 1 - (t + offset)%1 ) * BaseFreq ??
            
            // Let's calculate a "Cycle Offset" for 0, 1, 2
            float offset = i / 3.0f;
            float t = (progress + offset) % 1.0f; // 0 to 1
            
            // Downward Spiral: Pitch goes High -> Low
            // 2^1 (2x) down to 2^0 (1x)? Or 2^2 down to 2^-1?
            
            // Logarithmic Drop: 2^(2 * (1-t)) -> from 4x to 1x?
            // Let's try 2^(1 - t). Range: 2.0 -> 1.0.
            // If we use 3 voices, we cover 3 octaves.
            
            // Let's do a wider range: 2^(2 - 2t). Range: 4.0 -> 1.0.
            // Pitch Multiplier P = Mathf.Pow(2, 2.0f * (1.0f - t));
            
            // Actually, simplest Shepard is:
            // Freq(t) = Base * 2^t. 
            // We want loose range.
            
            float octavePos = (progress * 3.0f + i) % 3.0f; // 0.0 to 3.0
            // We want the pitch to drop. So invert progress.
            // float dropPos = 3.0f - octavePos; 
            
            // Let's stick to standard formula:
            // Relative frequency P = 2^x where x is in [-1, 2] ?
            
            // Let's refine based on "Infinite Descent":
            // 3 Voices.
            // Voice 0 starts at Pitch 4. Drops to 0.5.
            // Volume is peak at pitch 2-1, silent at 4 and 0.5.
            
            // Let's use a simpler parameter T which goes 0->1 repeatedly.
            // Voice `i` has effective T_i = (T + i/3) % 1.
            // PitchScale = Mathf.Pow(2, 2.0f - 3.0f * T_i); // Drops from 4 (2^2) to 0.5 (2^-1)
            // Volume = BellCurve(T_i);
            
            float T_i = (progress + (float)i/3.0f) % 1.0f;
            
            // Pitch: Drops from 4.0 to 0.5 (-1 octave)
            // 2^(2 - 3*t)
            // t=0 -> 2^2 = 4
            // t=1 -> 2^-1 = 0.5
            float exponent = 2.0f - (3.0f * T_i);
            float pitch = Mathf.Pow(2, exponent);
            
            _players[i].PitchScale = pitch;
            
            // Volume: Bell Curve (Hanning Window style)
            // Peak at t=0.5 (which is correct, middle of travel)
            // 0 -> Silence, 1 -> Silence.
            // Sin(t * PI)
            float vol = Mathf.Sin(T_i * Mathf.Pi);
            
            // Convert linear amplitude to db
            float db = Mathf.LinearToDb(vol);
            _players[i].VolumeDb = db - 10.0f; // Global attenuation
            
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
