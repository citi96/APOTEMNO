using Godot;
using System;
using System.Collections.Generic;
using Apotemno.Core.Narrative;

namespace Apotemno.Core;

// Defines the body parts that can be sacrificed.
public enum SacrificeType
{
    None,
    RightEye,     // Partial Blindness / UI Glitches
    TemporalLobe, // Memory Loss / Save Corruption
    RightArm,     // Weapon Handling Nerf / Interaction
    Legs,         // Forced Crawling
    VocalCords,   // Mute Mic / Narrative Silence
    Skin,         // Sensitivity / Damage Multiplier?
    LeftEar       // Mono Audio / Hallucinations
}

[GlobalClass]
public partial class SacrificeManagerGlobal : Node
{
    public static SacrificeManagerGlobal Instance { get; private set; }

    [Signal]
    public delegate void SacrificePerformedEventHandler(int typeInt); 

    private HashSet<SacrificeType> _performedSacrifices = new HashSet<SacrificeType>();
    private Dictionary<SacrificeType, SacrificeOption> _registry = new Dictionary<SacrificeType, SacrificeOption>();

    public override void _EnterTree()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadRegistry();
        }
        else
        {
            QueueFree();
        }
    }

    private void LoadRegistry()
    {
        // For now, we can load from a specific folder, or manually assign in Editor if we make this a scene.
        // Best practice for pure code Autoload: Load from "res://_Project/Resources/Sacrifices/" directory.
        // Assuming simple for now: distinct resources.
        // Implementation Note: Since we don't have the files yet, we will rely on manual registration or lazy loading.
        // Let's assume we expect them to be loaded.
        GD.Print("[LITURGY] Sacrifice Manager Initialized.");
    }
    
    // Helper to register an option (called by bootstrapper or loaded manually)
    public void RegisterOption(SacrificeOption option)
    {
        if (option == null) return;
        if (!_registry.ContainsKey(option.Type))
        {
            _registry.Add(option.Type, option);
        }
    }

    public bool HasSacrificed(SacrificeType type)
    {
        return _performedSacrifices.Contains(type);
    }
    
    public SacrificeOption GetSacrificeInfo(SacrificeType type)
    {
        if (_registry.ContainsKey(type)) return _registry[type];
        return null;
    }

    public void PerformSacrifice(SacrificeType type)
    {
        if (type == SacrificeType.None) return;

        if (_performedSacrifices.Contains(type))
        {
            GD.Print($"[LITURGY] Sacrifice of {type} already performed. Ignoring.");
            return;
        }

        GD.Print($"[LITURGY] SACRIFICE PERFORMED: {type}");
        _performedSacrifices.Add(type);

        // Notify the world
        EmitSignal(SignalName.SacrificePerformed, (int)type);

        // NARRATIVE REACTION
        if (NarrativeManagerGlobal.Instance != null)
        {
            // Simple hardcoded reaction for now, but could be moved to the Resource!
            if (type == SacrificeType.Legs)
            {
                 // Play reaction
            }
        }
        
        // Apply Gameplay Effects Immediately
        ApplySacrificeEffects(type);
    }
    
    private void ApplySacrificeEffects(SacrificeType type)
    {
        // Centralized effect application
        switch(type)
        {
            case SacrificeType.Legs:
                // Notification handled by PlayerController listening to signal usually, 
                // but we can also force it here if we have reference.
                break;
            case SacrificeType.TemporalLobe:
                // Wipe Saves
                if (Apotemno.Systems.SaveSystem.Instance != null)
                {
                    // Logic to corrupt saves
                    GD.Print("[LITURGY] Memories dissolving...");
                }
                break;
        }
    }
    
    public void ResetSacrifices()
    {
        _performedSacrifices.Clear();
        GD.Print("[LITURGY] All sins washed away (Debug Reset).");
    }
}
