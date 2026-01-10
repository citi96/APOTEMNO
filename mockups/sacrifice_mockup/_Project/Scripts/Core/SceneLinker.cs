using Godot;
using System;

namespace Apotemno.Core;

using Apotemno.Systems;

[GlobalClass]
public partial class SceneLinker : Node
{
    [Export] public Portal PortalA;
    [Export] public Portal PortalB;
    [Export] public Camera3D PlayerCamera; // Changed to Camera3D

    public override void _Ready()
    {
        CallDeferred(nameof(ConnectPortals));
    }

    private void ConnectPortals()
    {
        if (PortalA != null && PortalB != null)
        {
            // Link A -> B
            // PortalA.Destination = PortalB; // Portal.Destination is Node3D. Portal is Node3D. This works.
            
            // Link B -> A
            // PortalB.Destination = PortalA;

            // In 3D Portal.cs, we explicitly set Destination in Inspector usually. 
            // If runtime linking is needed:
            if (PortalA.Destination == null) PortalA.Destination = PortalB;
            if (PortalB.Destination == null) PortalB.Destination = PortalA;

            // PlayerCamera assignment removed as it was for 2D visual sync.
            // PortalA.PlayerCamera = PlayerCamera; 

            // FORCE PLAYER CAMERA DOMINANCE
            if (PlayerCamera != null)
            {
                PlayerCamera.Current = true; // Use .Current in 3D
                GD.Print("[SCENE LINKER] Player Camera set to CURRENT.");
            }

            GD.Print("[SCENE LINKER] Portals Connected Successfully (C# 3D).");
        }
        else
        {
            GD.PrintErr($"[SCENE LINKER] Missing References! A: {PortalA}, B: {PortalB}");
        }
    }
}
