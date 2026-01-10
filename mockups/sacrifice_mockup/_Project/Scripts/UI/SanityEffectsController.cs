using Godot;
using System;
using Apotemno.Systems;

namespace Apotemno.UI;

public partial class SanityEffectsController : CanvasLayer
{
    [Export] public ColorRect GlitchOverlay { get; set; }
    [Export] public TextureRect BSODImage { get; set; }
    [Export] public Label NotificationLabel { get; set; }

    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        if (SanitySystem.Instance != null)
        {
            SanitySystem.Instance.SanityPulse += OnSanityPulse;
            SanitySystem.Instance.MetaEventTriggered += OnMetaEvent;
        }

        if (GlitchOverlay != null) GlitchOverlay.Visible = false;
        if (BSODImage != null) BSODImage.Visible = false;
        if (NotificationLabel != null) NotificationLabel.Visible = false;
    }

    private async void OnSanityPulse(float intensity)
    {
        if (GlitchOverlay == null) return;

        // Flash glitch shader
        GlitchOverlay.Visible = true;
        // Assume ShaderMaterial is attached. Set params if precise control needed.
        // (GlitchOverlay.Material as ShaderMaterial)?.SetShaderParameter("shake_power", intensity);

        await ToSignal(GetTree().CreateTimer(_rng.RandfRange(0.1f, 0.3f)), SceneTreeTimer.SignalName.Timeout);
        
        GlitchOverlay.Visible = false;
    }

    private async void OnMetaEvent(string type)
    {
        if (type == "BSOD")
        {
            if (BSODImage == null) return;
            BSODImage.Visible = true;
            // Freeze frame?
            // OS.DelayMsec(500); // Only works if main thread, might freeze entire editor carefully. 
            // Better to just show image.
            
            await ToSignal(GetTree().CreateTimer(1.5f), SceneTreeTimer.SignalName.Timeout);
            BSODImage.Visible = false;
        }
        else if (type == "FILE_CREATED")
        {
            if (NotificationLabel == null) return;
            NotificationLabel.Text = "> SYSTEM ALERT: EXTERNAL FILE DETECTED";
            NotificationLabel.Visible = true;
            
            await ToSignal(GetTree().CreateTimer(3.0f), SceneTreeTimer.SignalName.Timeout);
            NotificationLabel.Visible = false;
        }
    }
}
