using Godot;
using Apotemno.Core.Narrative;
using Apotemno.Actors.Player;

namespace Apotemno.Systems.Narrative;

[GlobalClass]
public partial class NarrativeTrigger : Area2D
{
    [Export(PropertyHint.MultilineText)] 
    public string LineText = "Sample text";
    
    [Export] 
    public float Duration = 3.0f;
    
    [Export] 
    public bool OneShot = true;

    [Export]
    public bool InterruptPrevious = false;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is PlayerController)
        {
            if (NarrativeManagerGlobal.Instance != null)
            {
                // Simple string PlayLine
                NarrativeManagerGlobal.Instance.PlayLine(LineText, Duration, InterruptPrevious);
                
                if (OneShot)
                {
                    QueueFree();
                }
            }
        }
    }
}
