using Godot;
using Apotemno.Core;

namespace Apotemno.Levels;

public partial class LevelVentricleLogic : Node3D
{
    [Export] public Node3D ElevatorDoorVisuals;
    [Export] public CollisionShape3D ElevatorDoorCollider;
    [Export] public Node3D ElevatorPlatform; // Move this?

    private bool _elevatorUnlocked = false;

    public override void _Ready()
    {
        if (SacrificeManagerGlobal.Instance != null)
        {
            SacrificeManagerGlobal.Instance.SacrificePerformed += OnSacrificeperformed;
        }
    }

    public override void _ExitTree()
    {
        if (SacrificeManagerGlobal.Instance != null)
        {
            SacrificeManagerGlobal.Instance.SacrificePerformed -= OnSacrificeperformed;
        }
    }

    private void OnSacrificeperformed(int typeInt)
    {
        var type = (SacrificeType)typeInt;
        if (type == SacrificeType.ParietalLobe)
        {
             UnlockElevator();
        }
    }

    private void UnlockElevator()
    {
        GD.Print("[LEVEL] Parietal Lobe severed. The Elevator opens.");
        _elevatorUnlocked = true;
        
        // Open doors
        if (ElevatorDoorVisuals != null) ElevatorDoorVisuals.Visible = false;
        if (ElevatorDoorCollider != null) ElevatorDoorCollider.Disabled = true;
    }
}
