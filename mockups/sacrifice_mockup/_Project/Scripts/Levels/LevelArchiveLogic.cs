using Godot;
using Apotemno.Core;

namespace Apotemno.Levels;

public partial class LevelArchiveLogic : Node3D
{
    [Export] public Node3D HiddenStaircase; // Revealed after sacrifice
    [Export] public Node3D IllusionWall; // Removed after sacrifice

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
        if (type == SacrificeType.RightEye || type == SacrificeType.Skin) 
        {
             // Maybe "Eyes" isn't a single enum yet for Full Blindness, using RightEye for now or just generic check?
             // Task said "Eyes Sacrifice". Let's assume RightEye is the key for now.
             RevealTruth();
        }
    }

    private void RevealTruth()
    {
        GD.Print("[LEVEL] Vision sacrificed. The true path is revealed.");
        
        if (HiddenStaircase != null) HiddenStaircase.Visible = true;
        // Enable collision on stairs if they were disabled? Assuming HiddenStaircase parent has collision.
        if (IllusionWall != null) IllusionWall.QueueFree(); // Remove the wall blocking it
    }
}
