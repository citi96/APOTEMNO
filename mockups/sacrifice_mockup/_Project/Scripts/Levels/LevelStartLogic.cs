using Godot;
using Apotemno.Core;

namespace Apotemno.Levels;

public partial class LevelStartLogic : Node3D
{
    [Export] public Node3D DoorVisuals;
    [Export] public CollisionShape3D DoorCollider;
    [Export] public Node3D PitCover; // Visual floor that disappears

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
        if (type == SacrificeType.Blood)
        {
             OpenThePath();
        }
    }

    private void OpenThePath()
    {
        GD.Print("[LEVEL] Blood spilled. The path opens.");
        
        // Animate door opening or just delete for MVP
        if (DoorVisuals != null) DoorVisuals.Visible = false;
        if (DoorCollider != null) DoorCollider.Disabled = true;
        if (PitCover != null) PitCover.Visible = false;
        
        // Disable collision on pit cover if it exists (assuming it's a StaticBody container)
        if (PitCover?.GetParent() is CollisionObject3D col)
        {
            col.CollisionLayer = 0;
            col.CollisionMask = 0;
        }
    }
}
