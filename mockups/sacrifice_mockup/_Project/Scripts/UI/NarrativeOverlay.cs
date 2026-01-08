using Godot;
using Apotemno.Core.Narrative;

namespace Apotemno.UI;

public partial class NarrativeOverlay : CanvasLayer
{
    [Export]
    public RichTextLabel SubtitleLabel;

    private Tween _typewriterTween;

    public override void _Ready()
    {
        if (SubtitleLabel == null)
            SubtitleLabel = GetNode<RichTextLabel>("Control/SubtitleLabel");

        SubtitleLabel.Text = "";
        
        // Connect to Manager
        var manager = NarrativeManagerGlobal.Instance;
        if (manager != null)
        {
            manager.ShowLine += OnShowLine;
            manager.HideLine += OnHideLine;
        }
        else
        {
            GD.PrintErr("[UI] NarrativeManagerGlobal not found in NarrativeOverlay.");
        }
    }

    private void OnShowLine(string text, float duration)
    {
        SubtitleLabel.Text = text;
        SubtitleLabel.VisibleRatio = 0;

        _typewriterTween?.Kill();
        _typewriterTween = CreateTween();
        
        // Typewriter effect: 0 to 1 over a calculated time (faster for shorter text, but generally fast)
        float typeSpeed = text.Length * 0.05f; 
        if (typeSpeed > 1.5f) typeSpeed = 1.5f; // Cap at 1.5s max typing
        
        _typewriterTween.TweenProperty(SubtitleLabel, "visible_ratio", 1.0f, typeSpeed);
    }

    private void OnHideLine()
    {
        _typewriterTween?.Kill();
        SubtitleLabel.Text = "";
    }
}
