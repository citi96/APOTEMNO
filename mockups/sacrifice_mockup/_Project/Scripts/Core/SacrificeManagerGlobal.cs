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
    LeftEar,      // Mono Audio / Hallucinations
    Blood,        // Max HP Reduction
    ParietalLobe, // Input Latency / Inversion
    Heart         // Cardiac Arrest
}

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
        string path = "res://_Project/Resources/Sacrifices/";
        using var dir = DirAccess.Open(path);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (!dir.CurrentIsDir() && (fileName.EndsWith(".tres") || fileName.EndsWith(".remap")))
                {
                    // Handle .remap for exported builds if needed, but for now stick to .tres trimming
                    string loadPath = path + fileName.Replace(".remap", "");
                    var res = GD.Load<SacrificeOption>(loadPath);
                    if (res != null)
                    {
                        RegisterOption(res);
                        GD.Print($"[LITURGY] Registered Sacrifice: {res.DisplayName}");
                    }
                }
                fileName = dir.GetNext();
            }
        }
        else
        {
            GD.PrintErr($"[LITURGY] Failed to open sacrifice directory: {path}");
        }
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

    public List<SacrificeOption> GetAvailableSacrifices()
    {
        List<SacrificeOption> available = new List<SacrificeOption>();
        foreach(var kvp in _registry)
        {
            if (!HasSacrificed(kvp.Key))
            {
                available.Add(kvp.Value);
            }
        }
        return available;
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
            case SacrificeType.RightEye:
                // ... [Existing RightEye Code] ...
                var player = GetTree().GetFirstNodeInGroup("Player") as Actors.Player.PlayerController;
                player?.ApplyEyeSacrifice();
                break;

            case SacrificeType.Legs:
                // Notification handled by PlayerController listening to signal usually, 
                // but we can also force it here if we have reference.
                break;
            
            case SacrificeType.Skin:
                var skinPlayer = GetTree().GetFirstNodeInGroup("Player") as Actors.Player.PlayerController;
                skinPlayer?.ApplySkinSacrifice();
                break;

            case SacrificeType.TemporalLobe:
                 // Memory Sacrifice (Functionality in SaveSystem)
                 break;

            case SacrificeType.Blood:
                var healthBlood = GetTree().GetFirstNodeInGroup("HealthManager") as Apotemno.Systems.HealthManager;
                // Or find via Player if HealthManager is child? Let's try Global or Player path.
                // Assuming HealthManager is a child of Player for now based on scene structure.
                var pBlood = GetTree().GetFirstNodeInGroup("Player");
                var hBlood = pBlood?.GetNodeOrNull<Apotemno.Systems.HealthManager>("HealthManager");
                hBlood?.ApplyBloodSacrifice();
                break;

            case SacrificeType.ParietalLobe:
                var input = Apotemno.Core.InputBroker.Instance;
                if (input != null)
                {
                    input.InputLatencyFrames = 15; // 0.25s at 60fps
                    input.Unreliable = true;
                    input.Inverted = true; // "Invertire occasionalmente" -> Let's force it for impact, or use the Unreliable flag to toggle it.
                    GD.Print("[LITURGY] Parietal Lobe Sacrificed. Coordination Severed.");
                }
                break;

            case SacrificeType.Heart:
                var pHeart = GetTree().GetFirstNodeInGroup("Player");
                var hHeart = pHeart?.GetNodeOrNull<Apotemno.Systems.HealthManager>("HealthManager");
                hHeart?.EnableCardiacArrest();
                break;
        }
    }
    
    public void ResetSacrifices()
    {
        _performedSacrifices.Clear();
        GD.Print("[LITURGY] All sins washed away (Debug Reset).");
    }
}
