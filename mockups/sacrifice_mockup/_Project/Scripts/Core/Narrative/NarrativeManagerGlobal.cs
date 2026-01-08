using Godot;
using System.Collections.Generic;

namespace Apotemno.Core.Narrative;

public struct DialogueFragment
{
    public string Text;
    public AudioStream Audio;
    public float Duration;
    public bool IsInterrupting;

    public DialogueFragment(string text, float duration = 3.0f, AudioStream audio = null, bool interrupt = false)
    {
        Text = text;
        Duration = duration;
        Audio = audio;
        IsInterrupting = interrupt;
    }
}

[GlobalClass]
public partial class NarrativeManagerGlobal : Node
{
    public static NarrativeManagerGlobal Instance { get; private set; }

    [Signal]
    public delegate void ShowLineEventHandler(string text, float duration);

    [Signal]
    public delegate void HideLineEventHandler();

    private Queue<DialogueFragment> _queue = new Queue<DialogueFragment>();
    private bool _isPlaying = false;
    private AudioStreamPlayer _audioPlayer;

    public override void _EnterTree()
    {
        if (Instance == null)
            Instance = this;
        else
            QueueFree();
    }

    public override void _Ready()
    {
        _audioPlayer = new AudioStreamPlayer();
        _audioPlayer.Bus = "Intruder"; // Must match default_bus_layout.tres
        AddChild(_audioPlayer);
    }

    public void PlayLine(DialogueFragment fragment)
    {
        if (fragment.IsInterrupting)
        {
            _queue.Clear();
            _isPlaying = false;
            EmitSignal(SignalName.HideLine);
            // Small delay or immediate? Immediate for now.
        }

        _queue.Enqueue(fragment);
        ProcessQueue();
    }

    // Helper for strings only
    public void PlayLine(string text, float duration = 3.0f, bool interrupt = false)
    {
        PlayLine(new DialogueFragment(text, duration, null, interrupt));
    }

    private async void ProcessQueue()
    {
        if (_isPlaying || _queue.Count == 0) return;

        _isPlaying = true;
        var currentFragment = _queue.Dequeue();

        // Audio
        if (currentFragment.Audio != null)
        {
            _audioPlayer.Stream = currentFragment.Audio;
            _audioPlayer.Play();
        }

        // Visuals
        EmitSignal(SignalName.ShowLine, currentFragment.Text, currentFragment.Duration);

        // Wait
        await ToSignal(GetTree().CreateTimer(currentFragment.Duration), "timeout");

        // Hide
        EmitSignal(SignalName.HideLine);
        _isPlaying = false;
        
        // Next
        ProcessQueue();
    }
    
    public void ClearAll()
    {
        _queue.Clear();
        _isPlaying = false;
        _audioPlayer.Stop();
        EmitSignal(SignalName.HideLine);
    }
}
