using Godot;
using Apotemno.Core;

namespace Apotemno.UI;

public partial class DemiurgeDebugUI : CanvasLayer
{
    private Label _debugLabel;
    
    public override void _Ready()
    {
        _debugLabel = new Label();
        _debugLabel.Position = new Vector2(20, 20);
        _debugLabel.ThemeTypeVariation = "HeaderLarge";
        _debugLabel.Theme = new Theme(); // Default
        // Make it readable
        _debugLabel.AddThemeColorOverride("font_color", Colors.Magenta);
        _debugLabel.AddThemeConstantOverride("outline_size", 4);
        _debugLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
        
        AddChild(_debugLabel);
    }

    public override void _Process(double delta)
    {
        if (DemiurgeEngine.Instance == null) return;
        
        var d = DemiurgeEngine.Instance;
        var i = InputBroker.Instance;

        string txt = $"--- DEMIURGE DEBUG ---\n";
        txt += $"Pacing: {d.CurrentPacing}\n";
        txt += $"GlobalStress: {d.GlobalStressLevel:F2}\n";
        txt += $"\n--- METRICS (Raw) ---\n";
        txt += $"DirChanges: {i?.InputDirectionChanges}\n";
        txt += $"CamShake: {i?.CameraRotationAccumulator:F2}\n";

        _debugLabel.Text = txt;
    }
}
