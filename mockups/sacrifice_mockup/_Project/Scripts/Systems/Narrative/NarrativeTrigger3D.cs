using Godot;
using Apotemno.Actors.Player;
using Apotemno.Core.Narrative;

namespace Apotemno.Systems.Narrative;

[GlobalClass]
public partial class NarrativeTrigger3D : Area3D
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

    private void OnBodyEntered(Node3D body)
    {
        if (body is PlayerController)
        {
            if (NarrativeManagerGlobal.Instance != null)
            {
                NarrativeManagerGlobal.Instance.PlayLine(LineText, Duration, InterruptPrevious);

                if (OneShot)
                {
                    QueueFree();
                }
            }
        }
    }
}
