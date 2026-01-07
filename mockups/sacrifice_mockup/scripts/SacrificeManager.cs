using Godot;
using System;

namespace SacrificeMockup;

[GlobalClass]
public partial class SacrificeManager : Node
{
    [Export]
    public ColorRect SacrificeOverlay;

    private bool _isSacrificed = false;

    public override void _Ready()
    {
        if (SacrificeOverlay != null)
        {
            SacrificeOverlay.Visible = false;
        }
    }

    public override void _Process(double delta)
    {
        if (!_isSacrificed && Input.IsActionJustPressed("sacrifice"))
        {
            PerformSacrifice();
        }
    }

    private void PerformSacrifice()
    {
        _isSacrificed = true;
        GD.Print("SACRIFICE ACTIVATED: Half vision lost, Audio turning Mono.");

        if (SacrificeOverlay != null)
        {
            SacrificeOverlay.Visible = true;
        }

        // Audio effect simulation
        int masterBus = AudioServer.GetBusIndex("Master");
        if (masterBus >= 0)
        {
           GD.Print($"Audio Bus 'Master' found at index {masterBus}. Simulating Mono switch.");
        }
    }
}
