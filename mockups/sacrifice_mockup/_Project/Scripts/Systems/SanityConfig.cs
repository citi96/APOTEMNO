using Godot;
using System;

namespace Apotemno.Systems;

[GlobalClass]
public partial class SanityConfig : Resource
{
    [Export] public float MaxSanity { get; set; } = 100.0f;
    
    // Decay Rates (per second)
    [Export] public float BaseDecayRate { get; set; } = 0.5f;
    [Export] public float DarknessDecayRate { get; set; } = 2.0f;
    [Export] public float EnemyNearDecayRate { get; set; } = 5.0f;
    [Export] public float WhispersDecayRate { get; set; } = 3.0f;

    // Thresholds
    [Export] public float GlitchThreshold { get; set; } = 80.0f;
    [Export] public float HallucinationThreshold { get; set; } = 50.0f;
    [Export] public float CriticalThreshold { get; set; } = 20.0f;

    // Effects Duration
    [Export] public float GlitchDurationMin { get; set; } = 0.1f;
    [Export] public float GlitchDurationMax { get; set; } = 0.5f;
}
