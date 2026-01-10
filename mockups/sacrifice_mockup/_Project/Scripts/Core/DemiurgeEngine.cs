using Godot;
using System;
using Apotemno.Systems;

namespace Apotemno.Core;

[GlobalClass]
public partial class DemiurgeEngine : Node
{
    public static DemiurgeEngine Instance { get; private set; }

    // -- Configuration --
    private const float ANALYSIS_INTERVAL = 1.0f; // More frequent updates (1s) for continuous feel
    private const float STRESS_DECAY_RATE = 0.05f; // Natural stress decay per second
    private const float STRESS_PER_PANIC_EVENT = 0.15f; // Stress added per panic second
    
    // Pacing Thresholds
    private const float PEAK_THRESHOLD = 0.9f;
    private const float RELAX_THRESHOLD = 0.2f;
    private const float PEAK_DURATION_MAX = 10.0f; // Max time in Peak before forced Relax
    private const float RELAX_DURATION_MIN = 5.0f; // Min time in Relax before Build starts

    // -- State --
    public enum PacingState { BuildUp, Peak, Relax }
    public PacingState CurrentPacing { get; private set; } = PacingState.BuildUp;

    // The core metric: 0.0 (Calm) -> 1.0 (Terror)
    public float GlobalStressLevel { get; private set; } = 0.0f;

    private double _timer = 0;
    private double _stateTimer = 0; // Time in current state

    public override void _EnterTree()
    {
        if (Instance == null) Instance = this;
        else QueueFree();
    }

    public override void _Process(double delta)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused) return;

        UpdateStressModel((float)delta);
        UpdatePacingLogic((float)delta);
        
        // Pulse effects based on state
        ApplyDirectorEffects();
    }

    private void UpdateStressModel(float delta)
    {
        _timer += delta;
        if (_timer >= ANALYSIS_INTERVAL)
        {
            _timer = 0;
            CalculateInputStress();
        }

        // Natural Decay (Unless in Peak, maybe?)
        // In Relax mode, decay is accelerated drastically.
        float decayMult = (CurrentPacing == PacingState.Relax) ? 4.0f : 1.0f;
        
        GlobalStressLevel -= STRESS_DECAY_RATE * decayMult * delta;
        GlobalStressLevel = Mathf.Clamp(GlobalStressLevel, 0.0f, 1.0f);
    }

    private void CalculateInputStress()
    {
        if (InputBroker.Instance == null) return;

        int dirChanges = InputBroker.Instance.InputDirectionChanges;
        float camShake = InputBroker.Instance.CameraRotationAccumulator;
        bool sprinting = InputBroker.Instance.IsSprinting;

        // "Panic Inputs" add to Stress
        bool basicPanic = (dirChanges > 4) || (camShake > 8.0f); 
        
        // Sprinting is inherently stressful/panicked in this context
        // If sprinting, add a small continuous amount
        if (sprinting)
        {
             GlobalStressLevel += 0.10f; // Increased from 0.05
        }

        if (basicPanic)
        {
            GlobalStressLevel += 0.15f; // Increased from 0.05
            GD.Print($"[DEMIURGE] > Panic Input! Stress: {GlobalStressLevel:F2}");
        }

        InputBroker.Instance.ResetMetrics();
    }

    private void UpdatePacingLogic(float delta)
    {
        _stateTimer += delta;

        switch (CurrentPacing)
        {
            case PacingState.BuildUp:
                // Transition to Peak if Stress is high
                if (GlobalStressLevel >= PEAK_THRESHOLD)
                {
                    ChangePacing(PacingState.Peak);
                }
                // Transition to Relax if Stress is LOW for a long time (Recovery)
                else if (GlobalStressLevel < 0.1f && _stateTimer > 10.0f)
                {
                    ChangePacing(PacingState.Relax);
                }
                break;

            case PacingState.Peak:
                // Keep peak for a bit, but force Relax if too long OR if player manages to calm down perfectly
                if (_stateTimer > PEAK_DURATION_MAX) 
                {
                    ChangePacing(PacingState.Relax);
                }
                break;

            case PacingState.Relax:
                // Stay in Relax until Stress spikes or timer runs out
                // Force stress down, refuse to build up until timer done
                if (_stateTimer > RELAX_DURATION_MIN && GlobalStressLevel > RELAX_THRESHOLD)
                {
                   // Only exit Relax if we actually have some stress/reason to leave
                   ChangePacing(PacingState.BuildUp);
                }
                break;
        }
    }

    private void ChangePacing(PacingState newState)
    {
        CurrentPacing = newState;
        _stateTimer = 0;
        GD.Print($"[DEMIURGE] >>> PACING CHANGE: {newState} <<<");
    }

    private void ApplyDirectorEffects()
    {
        if (SanitySystem.Instance == null) return;

        // Modulate Sanity System based on Pacing
        switch (CurrentPacing)
        {
            case PacingState.BuildUp:
                // Normal Gameplay. Stress Level modulates Ambient Decay slightly.
                // Stress 0.5 -> 1.5x Decay.
                SanitySystem.Instance.SetExternalStress(1.0f + (GlobalStressLevel * 0.5f));
                break;

            case PacingState.Peak:
                // MAXIMUM PRESSURE.
                // Glitches are guaranteed/high probability.
                // Sanity drops fast.
                SanitySystem.Instance.SetExternalStress(3.0f); 
                break;

            case PacingState.Relax:
                // PURE RELIEF.
                // No Glitches.
                // No Sanity Decay (or healing?).
                SanitySystem.Instance.SetExternalStress(0.0f); 
                break;
        }
    }
}
