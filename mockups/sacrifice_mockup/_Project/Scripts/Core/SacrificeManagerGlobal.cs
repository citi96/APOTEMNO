using Godot;
using System;
using System.Collections.Generic;
using Apotemno.Core.Narrative;

namespace Apotemno.Core;

// Defines the body parts that can be sacrificed.
public enum SacrificeType
{
    None,
    Legs, // Forces Crawling
    Eyes, // Blinding / Dither intensity
    Voice, // Mutes microphone input (future)
    Hands // Disables interaction? (future)
}

[GlobalClass]
public partial class SacrificeManagerGlobal : Node
{
    public static SacrificeManagerGlobal Instance { get; private set; }

    [Signal]
    public delegate void SacrificePerformedEventHandler(int typeInt); 
    // Godot Signals don't love Enums directly in signatures nicely with C# sometimes, 
    // keeping it int for safety or we can cast. Let's try explicit enum but fallback to int if needed.
    // Actually, C# signals work fine with Enums usually.
    // public delegate void SacrificePerformedEventHandler(SacrificeType type);

    private HashSet<SacrificeType> _sacrifices = new HashSet<SacrificeType>();

    public override void _EnterTree()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree();
        }
    }

    public bool HasSacrificed(SacrificeType type)
    {
        return _sacrifices.Contains(type);
    }

    public void PerformSacrifice(SacrificeType type)
    {
        if (type == SacrificeType.None) return;

        if (_sacrifices.Contains(type))
        {
            GD.Print($"[LITURGY] Sacrifice of {type} already performed. Ignoring.");
            return;
        }

        GD.Print($"[LITURGY] SACRIFICE PERFORMED: {type}");
        _sacrifices.Add(type);

        // Notify the world
        EmitSignal(SignalName.SacrificePerformed, (int)type);

        // NARRATIVE REACTION
        if (NarrativeManagerGlobal.Instance != null)
        {
            if (type == SacrificeType.Legs)
            {
                // LOAD AUDIO
                AudioStream voiceClip = GD.Load<AudioStream>("res://_Project/Audio/Voices/test_voice.mp3");
                
                NarrativeManagerGlobal.Instance.PlayLine(
                    new DialogueFragment(
                        "[center][wave amp=50 freq=2]Li senti? Stanno ridendo di te.[/wave] Guarda come strisci. Patetico.[/center]", 
                        5.0f,
                        voiceClip,
                        true
                    )
                );
            }
        }
    }
    
    // Debug helper
    public void ResetSacrifices()
    {
        _sacrifices.Clear();
        GD.Print("[LITURGY] All sins washed away (Debug Reset).");
    }
}
