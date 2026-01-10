using Godot;
using System;
using Apotemno.Core;

namespace Apotemno.Systems;

[GlobalClass]
public partial class SanitySystem : Node
{
    public static SanitySystem Instance { get; private set; }

    [Export] public SanityConfig Config { get; set; }

    // Hidden State
    private float _currentSanity;
    
    // Decay Modifiers
    public bool IsInDarkness { get; set; }
    public bool IsEnemyNear { get; set; }
    public bool IsHearingWhispers { get; set; }

    // Signals for Effects (Internal use mostly, or for the Effects Controller)
    [Signal] public delegate void SanityPulseEventHandler(float intensity);
    [Signal] public delegate void MetaEventTriggeredEventHandler(string eventType);

    public override void _EnterTree()
    {
        if (Instance == null) Instance = this;
        else QueueFree();
    }

    public override void _Ready()
    {
        if (Config == null)
        {
            // Default config if none assigned
            Config = new SanityConfig();
        }
        _currentSanity = Config.MaxSanity;
    }

    public override void _Process(double delta)
    {
        UpdateSanity(delta);
    }

    // Director Control
    private float _externalStressMultiplier = 1.0f;

    public void SetExternalStress(float multiplier)
    {
        _externalStressMultiplier = multiplier;
    }

    private void UpdateSanity(double delta)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused) return;

        // Relax Mode Check (Multiplier 0) -> No Decay, maybe Heal?
        if (_externalStressMultiplier <= 0.01f)
        {
             // Optional: Slowly recover Sanity during Relax??
             // _currentSanity += 1.0f * (float)delta;
             // _currentSanity = Mathf.Clamp(_currentSanity, 0, Config.MaxSanity);
             return; 
        }

        float decay = Config.BaseDecayRate;

        if (IsInDarkness) decay += Config.DarknessDecayRate;
        if (IsEnemyNear) decay += Config.EnemyNearDecayRate;
        if (IsHearingWhispers) decay += Config.WhispersDecayRate;

        // Apply Director Multiplier
        decay *= _externalStressMultiplier;

        _currentSanity -= decay * (float)delta;
        _currentSanity = Mathf.Clamp(_currentSanity, 0, Config.MaxSanity);

        CheckThresholds();
    }

    private void CheckThresholds()
    {
        // Randomly trigger effects based on current sanity level
        // Lower sanity = higher probability
        
        // If in Relax State (Stress 0), suppress ALL effects
        if (_externalStressMultiplier <= 0.01f) return;

        float sanityPercent = _currentSanity / Config.MaxSanity;

        if (sanityPercent < (Config.GlitchThreshold / 100f))
        {
             // Base chance
             float chance = 0.05f;
             
             // If Stress is Peak (> 2.0), drastically increase pulse chance
             if (_externalStressMultiplier > 2.0f) chance = 0.2f; // 20% per frame is chaotic!

             if (GD.Randf() < chance) 
             {
                 EmitSignal(SignalName.SanityPulse, 0.2f * _externalStressMultiplier); 
             }
        }

        if (sanityPercent < (Config.CriticalThreshold / 100f))
        {
             // Critical Events
             /* 
             // BSOD TEMPORARILY DISABLED PER USER REQUEST
             if (GD.Randf() < 0.0005f) // Rare
             {
                 TriggerBSOD();
             }
             */

             if (GD.Randf() < 0.0001f) // Very Rare
             {
                 CreateMetaFile();
             }
        }
    }

    // --- Meta Events ---

    private bool _bsodTriggered = false;
    private async void TriggerBSOD()
    {
        if (_bsodTriggered) return; // Debounce
        _bsodTriggered = true;
        
        GD.Print("[SANITY] TRIGGERING BSOD!");
        // EmitSignal(SignalName.MetaEventTriggered, "BSOD"); // Disabled

        // Wait a bit to reset debounce
        await ToSignal(GetTree().CreateTimer(5.0f), SceneTreeTimer.SignalName.Timeout);
        _bsodTriggered = false;
    }

    private bool _fileCreated = false;
    private void CreateMetaFile()
    {
        if (_fileCreated) return; // Once per session? or cooldown?
        _fileCreated = true;

        GD.Print("[SANITY] Writing weird file to Disk...");
        
        string[] messages = {
            "DONT LOOK BEHIND YOU",
            "IT SEES YOU",
            "WAKE UP",
            "THEY ARE LYING"
        };
        
        string filename = $"help_me_{DateTime.Now.Ticks}.txt";
        string content = messages[GD.RandRange(0, messages.Length - 1)];

        // User data folder (AppData/Roaming/Godot/app_userdata/...)
        string path = "user://" + filename; 

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(content);
            file.Close();
            
            // Notify User in-game
            EmitSignal(SignalName.MetaEventTriggered, "FILE_CREATED");
            GD.Print($"[SANITY] File created: {ProjectSettings.GlobalizePath(path)}");
        }
    }

    // Helper to debug force sanity down
    public void DebugReduceSanity(float amount)
    {
        _currentSanity -= amount;
        GD.Print($"[SANITY] Debug Reduced to {_currentSanity}");
    }
}
