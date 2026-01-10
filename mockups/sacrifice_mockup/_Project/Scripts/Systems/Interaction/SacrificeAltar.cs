using Godot;
using Apotemno.Core;
using Apotemno.UI;

namespace Apotemno.Systems.Interaction;

[GlobalClass]
public partial class SacrificeAltar : StaticBody3D
{
    // The specific sacrifice required (optional, or opens general menu)
    [Export] public SacrificeType RequiredSacrifice { get; set; } = SacrificeType.None; 
    
    // If not None, we could restrict the menu to only show this option?
    // For now, let's just Open the Menu when interacted with.

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
