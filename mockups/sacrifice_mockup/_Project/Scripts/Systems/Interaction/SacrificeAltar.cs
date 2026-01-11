using Godot;
using Apotemno.Core;
using Apotemno.UI;

namespace Apotemno.Systems.Interaction;

[GlobalClass]
public partial class SacrificeAltar : StaticBody3D
{
    // The specific sacrifice required (optional, or opens general menu)
    [Export] public SacrificeType RequiredSacrifice { get; set; } = SacrificeType.None; 
    
    public override void _Ready()
    {
        // FORCE COLLISION SIZE PROGRAMMATICALLY
        // This fixes the issue where the scene file auto-deletes the size property.
        var col = GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
        if (col != null)
        {
            if (col.Shape == null) col.Shape = new BoxShape3D();
            col.Disabled = false; // Ensure it's active
            if (col.Shape is BoxShape3D box)
            {
                box.Size = new Vector3(1, 1, 1);
                // GD.Print($"[ALTAR] Enforced Altar Collision Size to (1,1,1) for {Name}");
            }
            // GD.Print($"[ALTAR] {Name} Layer: {CollisionLayer}, Mask: {CollisionMask}");
        }
    }

    public void OnInteract()
    {
        GD.Print("[ALTAR] Altar Interacted. Opening Liturgy...");
        
        // Find the UI (Doing a search for now, ideally referenced or via Event)
        var ui = GetTree().GetFirstNodeInGroup("SacrificeUI") as SacrificeUI;
        if (ui != null)
        {
            ui.Open();
        }
        else
        {
            GD.PrintErr("[ALTAR] SacrificeUI not found in scene!");
        }
    }
}
