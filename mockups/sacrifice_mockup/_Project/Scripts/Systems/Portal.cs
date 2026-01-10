using Godot;
using System;
using Apotemno.Core; // For GameManager

namespace Apotemno.Systems;

[GlobalClass]
public partial class Portal : Area3D
{
    [Export(PropertyHint.File, "*.tscn")] public string TargetScenePath { get; set; }
    [Export] public string TargetSpawnPointName { get; set; } = "SpawnPoint"; // Name of Node3D in next scene

    // For Intra-Scene Teleportation (Non-Euclidean / Loops)
    public Node3D Destination { get; set; }

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is Actors.Player.PlayerController)
        {
            GD.Print($"[PORTAL] Player entered portal.");
            CallDeferred(nameof(Teleport), body);
        }
    }

    private void Teleport(Node3D body)
    {
        // 1. Check for Local Destination (Intra-Scene)
        if (Destination != null)
        {
            GD.Print($"[PORTAL] Teleporting to Local Destination: {Destination.Name}");
            // Simple transform teleport
            body.GlobalPosition = Destination.GlobalPosition;
            // Optionally match rotation or use relative transform for seamlessness
            return;
        }

        // 2. Scene Switch
        if (!string.IsNullOrEmpty(TargetScenePath))
        {
            GD.Print($"[PORTAL] Switching Scene to: {TargetScenePath}");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadLevel(TargetScenePath);
            }
            else
            {
                GetTree().ChangeSceneToFile(TargetScenePath);
            }
        }
    }
}
