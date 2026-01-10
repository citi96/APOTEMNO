using Godot;
using Apotemno.Core;

namespace Apotemno.Levels;

public partial class LevelOssuaryLogic : Node3D
{
    [Export] public Node3D FinalPortal; // Activates on heart sacrifice

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
        if (type == SacrificeType.Heart)
        {
             OpenFinalGate();
        }
    }

    private void OpenFinalGate()
    {
        GD.Print("[LEVEL] Heart stopped. The Finality awaits.");
        if (FinalPortal != null) FinalPortal.Visible = true;
        // Logic to maybe start pulsating screen effects here too via HUD
    }
}
