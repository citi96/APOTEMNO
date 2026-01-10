using Godot;
using Apotemno.Actors.Player;

namespace Apotemno.Systems;

[GlobalClass]
public partial class Portal : Node3D // Changed to Node3D
{
    [Export]
    public Node3D Destination; 

    [Export]
    public Area3D TeleportArea;

    public override void _Ready()
    {
        if (TeleportArea != null)
        {
            TeleportArea.BodyEntered += OnBodyEntered;
        }
        // Camera linking disabled for 3D refactor initially - requires RenderTarget/Mesh setup
    }

    private static ulong _lastTeleportTime = 0;
    private const ulong TELEPORT_COOLDOWN_MS = 200;

    public override void _Process(double delta)
    {
        // Debug
        if (Destination == null && Engine.GetFramesDrawn() % 60 == 0)
        {
            GD.PrintErr($"[PORTAL] {Name}: DESTINATION IS NULL!");
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (body is PlayerController player && Destination != null)
        {
            ulong now = Time.GetTicksMsec();
            if (now - _lastTeleportTime < TELEPORT_COOLDOWN_MS) return;

            GD.Print($"[PORTAL] Teleporting {body.Name} to {Destination.Name}");
            // Offset logic in 3D
            Vector3 entryOffset = player.GlobalPosition - this.GlobalPosition;
            // Simple teleport: just match GlobalPosition + Offset
            // Note: Rotation matching is more complex (basis transformation), skipping for now.
            player.GlobalPosition = Destination.GlobalPosition + entryOffset;
            _lastTeleportTime = now;
        }
    }
}
